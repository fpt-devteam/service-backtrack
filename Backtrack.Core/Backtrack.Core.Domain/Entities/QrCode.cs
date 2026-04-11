namespace Backtrack.Core.Domain.Entities;

public sealed class QrCode : Entity<Guid>
{
    public required string UserId { get; set; }
    public required string PublicCode { get; set; } // BTK-XXXXXXXX
    public string? Note { get; set; }
}
