namespace Backtrack.Core.Domain.Constants;

public static class EventTopics
{
    public static class User
    {
        public const string EnsureExist = "user.ensure-exist";
    }

    public static class Invitation
    {
        public const string Created = "invitation.created";
    }

    public const string ReturnReportConfirmed = "return-report.confirmed";
    public const string ReturnReportSynced = "return-report.synced";

    public static class Org
    {
        public const string EnsureExist = "org.ensure-exist";
    }
}
