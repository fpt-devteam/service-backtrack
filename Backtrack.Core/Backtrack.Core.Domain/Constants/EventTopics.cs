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
}
