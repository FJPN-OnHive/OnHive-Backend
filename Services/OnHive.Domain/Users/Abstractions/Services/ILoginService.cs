using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Users.Domain.Abstractions.Services
{
    public interface ILoginService
    {
        Task<LoginResponseDto> LoginBasicExpiredAsync(LoginDto login);

        Task<LoginResponseDto> LoginAsync(LoginDto login);

        Task<LoginResponseDto> ImpersonateAsync(string userId);

        Task RequestPasswordRecoverAsync(string email, string tenantId);

        Task PasswordRecoverAsync(RecoverPasswordDto recoverPassword);

        Task<LoginResponseDto> RefreshToken(string token, UserDto loggedUser);
    }
}