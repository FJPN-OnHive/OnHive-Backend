using EHive.Core.Library.Contracts.Login;

namespace EHive.Admin.Services
{
    public interface ILoginService
    {
        Task<LoginResponseDto> Login(LoginDto login);
    }
}