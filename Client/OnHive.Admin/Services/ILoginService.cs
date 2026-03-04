using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Admin.Services
{
    public interface ILoginService
    {
        Task<LoginResponseDto> Login(LoginDto login);
    }
}