namespace Backtrack.Core.Application.Exceptions.Errors;

public static class ReturnReportErrors
{
    public static readonly Error NotFound = new(
        Code: "ReturnReportNotFound",
        Message: "ReturnReport not found.");

    public static readonly Error InvalidToken = new(
        Code: "InvalidReturnReportToken",
        Message: "The ReturnReport token is invalid or expired.");

    public static readonly Error AlreadyExists = new(
        Code: "ReturnReportAlreadyExists",
        Message: "An active ReturnReport already exists for these posts.");

    public static readonly Error FinderPostAlreadyInReport = new(
        Code: "FinderPostAlreadyInReport",
        Message: "This finder post already has an active return report.");

    public static readonly Error OwnerPostAlreadyInReport = new(
        Code: "OwnerPostAlreadyInReport",
        Message: "This owner post already has an active return report.");

    public static readonly Error AlreadyConfirmed = new(
        Code: "ReturnReportAlreadyConfirmed",
        Message: "This ReturnReport has already been confirmed.");

    public static readonly Error AlreadyRejected = new(
        Code: "ReturnReportAlreadyRejected",
        Message: "This ReturnReport has already been rejected.");

    public static readonly Error AlreadyExpired = new(
        Code: "ReturnReportAlreadyExpired",
        Message: "This ReturnReport has expired.");

    public static readonly Error OwnerNotConfirmed = new(
        Code: "OwnerNotConfirmed",
        Message: "Owner has not confirmed this ReturnReport yet.");

    public static readonly Error StaffNotAuthorized = new(
        Code: "StaffNotAuthorized",
        Message: "You are not authorized to confirm this ReturnReport.");

    public static readonly Error InvalidReturnReportType = new(
        Code: "InvalidReturnReportType",
        Message: "This operation is not valid for this ReturnReport type.");

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

    public static readonly Error CannotReturnReportOwnPost = new(
        Code: "CannotReturnReportOwnPost",
        Message: "You cannot create a ReturnReport for your own post.");

    public static readonly Error PostNotInOrg = new(
        Code: "PostNotInOrg",
        Message: "This post does not belong to your organization.");

    public static readonly Error PostTypeMismatch = new(
        Code: "PostTypeMismatch",
        Message: "Finder post must be FOUND type and owner post must be LOST type.");

    public static readonly Error OwnerIdRequired = new(
        Code: "OwnerIdRequired",
        Message: "OwnerId is required when OwnerPostId is not provided.");

    public static readonly Error FinderIdRequired = new(
        Code: "FinderIdRequired",
        Message: "FinderId is required when FinderPostId is not provided.");

    public static readonly Error OwnerNotFound = new(
        Code: "OwnerNotFound",
        Message: "Owner user not found.");

    public static readonly Error FinderNotFound = new(
        Code: "FinderNotFound",
        Message: "Finder user not found.");

    public static readonly Error NotParticipant = new(
        Code: "NotParticipant",
        Message: "You must be the finder or owner of one of the provided posts to create a return report.");
}
