using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events;
using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Data;

public sealed class TicketingDbContext : DbContext, IUnitOfWork
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();
    public DbSet<TicketPurchase> TicketPurchases => Set<TicketPurchase>();

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken)
    {
        await using var transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.Venue).IsRequired().HasMaxLength(300);
            entity.Property(x => x.TotalCapacity).IsRequired();
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.Metadata.FindNavigation(nameof(Event.PricingTiers))!.SetPropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany<PricingTier>(x => x.PricingTiers)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PricingTier>(entity =>
        {
            entity.ToTable("PricingTiers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EventId, x.Name }).IsUnique();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Price).HasConversion<double>().IsRequired();
            entity.Property(x => x.Capacity).IsRequired();
            entity.Property(x => x.SoldQuantity).IsRequired();
            entity.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<TicketPurchase>(entity =>
        {
            entity.ToTable("TicketPurchases");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.PricingTierId);
            entity.Property(x => x.BuyerName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.BuyerEmail).IsRequired().HasMaxLength(300);
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.UnitPrice).HasConversion<double>().IsRequired();
            entity.Property(x => x.TotalAmount).HasConversion<double>().IsRequired();
            entity.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PricingTier)
                .WithMany()
                .HasForeignKey(x => x.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
