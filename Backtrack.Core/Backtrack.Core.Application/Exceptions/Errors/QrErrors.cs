namespace Backtrack.Core.Application.Exceptions.Errors;

public static class QrErrors
{
    public static readonly Error NotFound = new(
        Code: "QrCodeNotFound",
        Message: "QR code not found.");

    public static readonly Error Forbidden = new(
        Code: "QrCodeForbidden",
        Message: "You are not authorized to perform this action on this QR code.");

    public static readonly Error DesignNotFound = new(
        Code: "QrDesignNotFound",
        Message: "QR design not found.");
}
