namespace Backtrack.Core.Application.Common;

/// <summary>
/// Maps an organization's active subscription plan name to its member seat limit.
/// null limit means unlimited seats (Max tier).
/// </summary>
public static class OrgMemberLimitPolicy
{
    public const int FreeMemberLimit = 3;
    public const int ProMemberLimit  = 20;

    /// <summary>
    /// Returns the seat limit for the given plan name.
    /// Returns null for unlimited plans (Max tier).
    /// Returns FreeMemberLimit when planName is null (no active subscription).
    /// </summary>
    public static int? GetLimit(string? planName) => planName switch
    {
        null                                                                    => FreeMemberLimit,
        var n when n.Contains("Max", StringComparison.OrdinalIgnoreCase)       => null,
        var n when n.Contains("Pro", StringComparison.OrdinalIgnoreCase)       => ProMemberLimit,
        _                                                                       => FreeMemberLimit,
    };
}
