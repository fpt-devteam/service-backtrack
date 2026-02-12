namespace Backtrack.Core.Application.Exceptions.Errors;

public static class InvitationErrors
{
    public static readonly Error InvitationNotFound = new(
        Code: "InvitationNotFound",
        Message: "Invitation not found.");

    public static readonly Error InvitationExpired = new(
        Code: "InvitationExpired",
        Message: "This invitation has expired.");

    public static readonly Error InvitationAlreadyAccepted = new(
        Code: "InvitationAlreadyAccepted",
        Message: "This invitation has already been accepted.");

    public static readonly Error InvitationNotPending = new(
        Code: "InvitationNotPending",
        Message: "This invitation is no longer pending.");

    public static readonly Error EmailMismatch = new(
        Code: "EmailMismatch",
        Message: "Your email does not match the invitation email.");

    public static readonly Error AlreadyInvited = new(
        Code: "AlreadyInvited",
        Message: "A pending invitation already exists for this email in this organization.");
}
