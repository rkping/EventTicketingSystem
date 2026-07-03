using EventTicketing.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventTicketing.Tests.Common.Fixtures;

public sealed class DbContextFixture : IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private TicketingDbContext? _dbContext;

    public TicketingDbContext DbContext => _dbContext ?? throw new InvalidOperationException("DbContext not initialized");

    public DbContextFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new TicketingDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        await _connection.CloseAsync();
        _connection.Dispose();
    }
}
