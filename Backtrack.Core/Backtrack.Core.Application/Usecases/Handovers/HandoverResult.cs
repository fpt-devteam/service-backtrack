using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Handovers;

public sealed record HandoverResult
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public Guid? FinderPostId { get; init; }
    public Guid? OwnerPostId { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public HandoverOrgExtensionResult? OrgExtension { get; init; }
}

public sealed record HandoverOrgExtensionResult
{
    public required Guid Id { get; init; }
    public required Guid OrgId { get; init; }
    public required string StaffId { get; init; }
    public required bool OwnerVerified { get; init; }
    public Dictionary<string, string>? OwnerFormData { get; init; }
    public DateTimeOffset? StaffConfirmedAt { get; init; }
    public DateTimeOffset? OwnerConfirmedAt { get; init; }
}

public sealed record HandoverDetailResult
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public Guid? FinderPostId { get; init; }
    public Guid? OwnerPostId { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public HandoverOrgExtensionResult? OrgExtension { get; init; }
    public List<FormFieldDefinition>? FormTemplate { get; init; }
}
