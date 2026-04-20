namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record PersonalBelongingDetailInput
{
    public string? ItemName { get; init; }
    public string? Color { get; init; }
    public string? Brand { get; init; }
    public string? Material { get; init; }
    public string? Size { get; init; }
    public string? Condition { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string? AiDescription { get; init; }
    public string? AdditionalDetails { get; init; }
}

public sealed record CardDetailInput
{
    public string? ItemName { get; init; }
    public string? CardNumber { get; init; }
    public string? HolderName { get; init; }
    public string? HolderNameNormalized { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public string? IssuingAuthority { get; init; }
    public string? OcrText { get; init; }
    public string? AdditionalDetails { get; init; }
}

public sealed record ElectronicDetailInput
{
    public string? ItemName { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Color { get; init; }
    public bool? HasCase { get; init; }
    public string? CaseDescription { get; init; }
    public string? ScreenCondition { get; init; }
    public string? LockScreenDescription { get; init; }
    public string? DistinguishingFeatures { get; init; }
    public string? AiDescription { get; init; }
    public string? AdditionalDetails { get; init; }
}

public sealed record OtherDetailInput
{
    public required string ItemIdentifier { get; init; }
    public string? PrimaryColor { get; init; }
    public string? AiDescription { get; init; }
    public string? AdditionalDetails { get; init; }
}
