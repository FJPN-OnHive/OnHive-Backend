using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Core.Library.Contracts.Students
{
    public class StudentUserDto
    {
        public StudentDto Student { get; set; }

        public UserDto User { get; set; }
    }
}