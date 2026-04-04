namespace Backtrack.Core.Application.Exceptions.Errors;

public static class HandoverErrors
{
    public static readonly Error NotFound = new(
        Code: "HandoverNotFound",
        Message: "Handover not found.");

    public static readonly Error InvalidToken = new(
        Code: "InvalidHandoverToken",
        Message: "The handover token is invalid or expired.");

    public static readonly Error AlreadyExists = new(
        Code: "HandoverAlreadyExists",
        Message: "An active handover already exists for these posts.");

    public static readonly Error AlreadyConfirmed = new(
        Code: "HandoverAlreadyConfirmed",
        Message: "This handover has already been confirmed.");

    public static readonly Error AlreadyExpired = new(
        Code: "HandoverAlreadyExpired",
        Message: "This handover has expired.");

    public static readonly Error OwnerNotConfirmed = new(
        Code: "OwnerNotConfirmed",
        Message: "Owner has not confirmed this handover yet.");

    public static readonly Error StaffNotAuthorized = new(
        Code: "StaffNotAuthorized",
        Message: "You are not authorized to confirm this handover.");

    public static readonly Error InvalidHandoverType = new(
        Code: "InvalidHandoverType",
        Message: "This operation is not valid for this handover type.");

    public static readonly Error MissingRequiredFormField = new(
        Code: "MissingRequiredFormField",
        Message: "Required form field is missing.");

    public static Error MissingFormField(string fieldKey) => new(
        Code: "MissingFormField",
        Message: $"Required form field '{fieldKey}' is missing.");

    public static readonly Error FinderPostNotFound = new(
        Code: "FinderPostNotFound",
        Message: "Finder post not found.");

    public static readonly Error OwnerPostNotFound = new(
        Code: "OwnerPostNotFound",
        Message: "Owner post not found.");

    public static readonly Error CannotHandoverOwnPost = new(
        Code: "CannotHandoverOwnPost",
        Message: "You cannot create a handover for your own post.");

    public static readonly Error PostTypeMismatch = new(
        Code: "PostTypeMismatch",
        Message: "Finder post must be FOUND type and owner post must be LOST type.");

    public static readonly Error OwnerIdRequired = new(
        Code: "OwnerIdRequired",
        Message: "OwnerId is required when OwnerPostId is not provided.");

    public static readonly Error OwnerNotFound = new(
        Code: "OwnerNotFound",
        Message: "Owner user not found.");
}
