namespace Backtrack.Core.Application.Exceptions.Errors;

public static class AiErrors
{
    public static readonly Error Busy = new(
        Code: "AiBusy",
        Message: "AI service is currently busy. Please try again in a moment.");

    public static readonly Error InvalidResponse = new(
        Code: "AiInvalidResponse",
        Message: "AI returned an unexpected response. Please try again.");
}
