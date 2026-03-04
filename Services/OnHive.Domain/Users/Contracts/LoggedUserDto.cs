using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Core.Library.Contracts.Login
{
    public class LoggedUserDto
    {
        public LoggedUserDto(UserDto? user, string token)
        {
            User = user;
            Token = token;
        }

        public UserDto? User { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}