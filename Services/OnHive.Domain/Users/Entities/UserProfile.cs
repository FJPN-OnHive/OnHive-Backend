using OnHive.Core.Library.Enums.Users;

namespace OnHive.Core.Library.Entities.Users
{
    public class UserProfile : EntityBase
    {
        public ProfileTypes Type { get; set; } = ProfileTypes.Student;

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public string? CoverPictureUrl { get; set; }

        public bool PublicEmail { get; set; } = false;
    }
}