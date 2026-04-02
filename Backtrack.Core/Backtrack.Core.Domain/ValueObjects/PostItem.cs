using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.ValueObjects
{
    public class PostItem
    {
        public required string ItemName { get; set; }
        public ItemCategory Category { get; set; } = ItemCategory.Other;
        public string? Color { get; set; }
        public string? Brand { get; set; }
        public string? Condition { get; set; }
        public string? Material { get; set; }
        public string? Size { get; set; }
        public string? DistinctiveMarks { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}