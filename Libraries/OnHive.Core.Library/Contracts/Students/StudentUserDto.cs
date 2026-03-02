using EHive.Core.Library.Contracts.Users;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentUserDto
    {
        public StudentDto Student { get; set; }

        public UserDto User { get; set; }
    }
}