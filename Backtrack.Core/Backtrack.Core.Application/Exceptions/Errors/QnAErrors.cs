namespace Backtrack.Core.Application.Exceptions.Errors;

public static class QnAErrors
{
    public static readonly Error NotFound = new(
        Code: "QuestionNotFound",
        Message: "Question not found.");

    public static readonly Error Forbidden = new(
        Code: "QuestionForbidden",
        Message: "You are not authorized to perform this action on this question.");

    public static readonly Error OnlyFoundPostsAllowed = new(
        Code: "QuestionOnlyFoundPostsAllowed",
        Message: "Questions can only be created on posts of type 'Found'.");
}

public static class AnswerErrors
{
    public static readonly Error NotFound = new(
        Code: "AnswerNotFound",
        Message: "Answer not found.");
}
