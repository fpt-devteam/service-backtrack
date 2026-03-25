using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.ValueObjects;

public sealed record DailySchedule
{
    public required WeekDay Day { get; init; }
    public bool IsClosed { get; init; }
    public string? OpenTime { get; init; }  // "09:00"
    public string? CloseTime { get; init; } // "18:00"
}
