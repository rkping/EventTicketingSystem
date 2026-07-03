using EventTicketing.Domain.Entities;

namespace EventTicketing.Api
{
    public class Event
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public string Venue { get; private set; } = default!;

        public DateOnly EventDate { get; private set; }
        public TimeOnly EventTime { get; private set; }

        public int TotalCapacity { get; private set; }

        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset? UpdatedAtUtc { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;

        public ICollection<PricingTier> PricingTiers { get; private set; } = new List<PricingTier>();
    }
}
