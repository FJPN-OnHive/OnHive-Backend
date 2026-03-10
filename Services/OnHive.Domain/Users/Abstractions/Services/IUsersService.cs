using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Enums.Users;
using System.Text.Json;

namespace OnHive.Users.Domain.Abstractions.Services
{
    public interface IUsersService
    {
        Task<string> GetLastValidationCodeAsync(string userId, string email, LoggedUserDto loggedUser);

        Task<UserDto?> GetByIdAsync(string userId, LoggedUserDto loggedUser);

        Task<UserDto?> GetByIdAsync(string userId);

        Task<IEnumerable<UserDto>> GetAllAsync(LoggedUserDto loggedUser);

        Task<PaginatedResult<UserDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<UserDto> CreateAsync(UserDto newUser, LoggedUserDto loggedUser);

        Task<UserDto> CreateAsync(SignInUserDto newUser);

        Task<UserDto> CreateWithRolesAsync(SignInUserDto newUser, List<string> roles);

        Task<UserDto?> UpdateAsync(UserDto userDto, LoggedUserDto loggedUser);

        Task<UserDto?> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<UserDto?> UpdateRolesAsync(UserDto userDto, LoggedUserDto loggedUser);

        Task<bool> AddEmailsAsync(string userId, string email, LoggedUserDto loggedUser);

        Task<bool> RemoveEmailsAsync(string userId, string email, LoggedUserDto loggedUser);

        Task<bool> SetMainEmailsAsync(string userId, string email, LoggedUserDto loggedUser);

        Task<UserDto> ValidateEmailAsync(string code, string tenantId);

        Task<UserDto> ValidateEmailAsync(string email, string userId, string tenantId);

        Task ResendEmailValidationAsync(string userId, string email, string tenantId);

        Task ResendMainEmailValidationAsync(string email, string tenantId);

        Task ChangePasswordAsync(ChangePasswordDto changePassword, LoggedUserDto loggedUser);

        Task<UserDto?> GetByLogin(string userLogin, LoggedUserDto loggedUser);

        Task<UserDto?> GetByMainEmail(string userEmail, LoggedUserDto loggedUser);

        Task Migrate(bool isProduction);

        Task DeactivateUser(string userId, LoggedUserDto loggedUser);

        Task ReactivateUser(string userId, LoggedUserDto loggedUser);

        Task Anonymize(string userId, LoggedUserDto loggedUser);

        Task DeleteAccountDataAsync(string userId, LoggedUserDto loggedUser);

        Task<UserDto?> AddAddressAsync(string userId, AddressDto address, LoggedUserDto loggedUser);

        Task<UserDto?> SetMainAddressAsync(string userId, string addressName, LoggedUserDto loggedUser);

        Task<UserDto?> UpdateAddressAsync(string userId, AddressDto address, LoggedUserDto loggedUser);

        Task<UserDto?> RemoveAddressAsync(string userId, string addressName, LoggedUserDto loggedUser);

        Task<PaginatedResult<UserDto>> GetByFilterAndProfileAsync(RequestFilter filter, ProfileTypes profile, LoggedUserDto loggedUser);

        Task<PaginatedResult<UserDto>> GetByIdsAsync(List<string> usersIds, RequestFilter filter, UserDto loggedUser);

        Task<string> CreateTempPasswordAsync(string userId, LoggedUserDto loggedUser);
    }
}