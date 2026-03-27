using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.ValueObjects;

public sealed record FormFieldDefinition
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public required FormFieldType Type { get; init; }
    public required bool Required { get; init; }
    public List<string>? Options { get; init; }
}
