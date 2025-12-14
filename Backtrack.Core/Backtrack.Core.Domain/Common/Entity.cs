namespace Backtrack.Core.Domain.Common
{
    public abstract class Entity<TKey>
    {
        public TKey Id { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}