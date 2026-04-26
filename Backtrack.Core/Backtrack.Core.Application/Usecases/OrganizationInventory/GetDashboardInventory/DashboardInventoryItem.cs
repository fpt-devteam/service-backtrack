namespace Backtrack.Core.Application.Usecases.OrganizationInventory.GetDashboardInventory;

public sealed record DashboardInventoryItem
{
    public required Guid            Id               { get; init; }
    public required string          PostTitle        { get; init; }
    public required string          Category         { get; init; }
    public required string          SubcategoryName  { get; init; }
    public required string          Status           { get; init; }
    public required string          InternalLocation { get; init; }
    public          string?         ImageUrl         { get; init; }
    public required DateTimeOffset  CreatedAt        { get; init; }
}
