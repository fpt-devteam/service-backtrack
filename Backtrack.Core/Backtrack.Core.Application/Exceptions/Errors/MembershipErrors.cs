namespace Backtrack.Core.Application.Exceptions.Errors;

public static class MembershipErrors
{
    public static readonly Error NotAMember = new(
        Code: "NotAMember",
        Message: "You are not a member of this organization.");

    public static readonly Error InsufficientRole = new(
        Code: "InsufficientRole",
        Message: "You do not have the required role to perform this action.");

    public static readonly Error AlreadyAMember = new(
        Code: "AlreadyAMember",
        Message: "User is already a member of this organization.");

    public static readonly Error CannotRemoveLastAdmin = new(
        Code: "CannotRemoveLastAdmin",
        Message: "Cannot remove the last active admin of the organization.");

    public static readonly Error CannotDemoteLastAdmin = new(
        Code: "CannotDemoteLastAdmin",
        Message: "Cannot change role of the last active admin of the organization.");

    public static readonly Error MemberNotFound = new(
        Code: "MemberNotFound",
        Message: "The specified member was not found in this organization.");

    public static readonly Error InvalidMembershipRole = new(
        Code: "InvalidMembershipRole",
        Message: "MembershipRole must be either 'OrgAdmin' or 'OrgStaff'.");
}
