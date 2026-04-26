namespace Backtrack.Core.Application.Exceptions.Errors;

public static class NotificationErrors
{
    public static readonly Error DeviceNotFound = new("DEVICE_NOT_FOUND", "Device not found.");
    public static readonly Error NotificationNotFound = new("NOTIFICATION_NOT_FOUND", "Notification not found.");
}
