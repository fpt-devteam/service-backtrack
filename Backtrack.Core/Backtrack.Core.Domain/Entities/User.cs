using Backtrack.Core.Domain.Common;
using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities
{
    public class User : Entity<string>
    {
        public required string Email { get; set; }
        public string? DisplayName { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public UserRole Role { get; set; } = UserRole.User;
    }
}