namespace Backtrack.Core.Application.Exceptions.Errors;

public static class QnAErrors
{
    public static readonly Error NotFound = new(
        Code: "QnANotFound",
        Message: "Question not found.");

    public static readonly Error Forbidden = new(
        Code: "QnAForbidden",
        Message: "You are not authorized to perform this action on this question.");

    public static readonly Error AlreadyAnswered = new(
        Code: "QnAAlreadyAnswered",
        Message: "This question has already been answered.");
}
