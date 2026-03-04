using AutoMapper;
using OnHive.Authorization.Library.Models;
using OnHive.Configuration.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Configuration;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Extensions;
using OnHive.Core.Library.Helpers;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Users.Domain.Abstractions.Repositories;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Users.Domain.Exceptions;
using OnHive.Users.Domain.Models;
using OnHive.Users.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnHive.Domains.Common.Helpers;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnHive.Users.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUsersRepository usersRepository;
        private readonly IRolesRepository rolesRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IEmailsService emailsService;
        private readonly ITenantsService tenantsService;
        private readonly IConfigurationService configurationService;

        public LoginService(IUsersRepository usersRepository,
                            IRolesRepository rolesRepository,
                            UsersApiSettings usersApiSettings,
                            IMapper mapper,
                            IEmailsService emailsService,
                            ITenantsService tenantsService,
                            IConfigurationService configurationService)
        {
            this.usersRepository = usersRepository;
            this.rolesRepository = rolesRepository;
            this.usersApiSettings = usersApiSettings;
            this.mapper = mapper;
            this.emailsService = emailsService;
            this.tenantsService = tenantsService;
            this.configurationService = configurationService;
            logger = Log.Logger;
        }

        public async Task<LoginResponseDto> LoginBasicExpiredAsync(LoginDto login)
        {
            var user = await GetUser(login);
            ValidateLogin(login, user);
            await ValidateAppPermission(login, user);
            return await GetLoginResponseAsync(user, login.RemindMe, true);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto login)
        {
            var user = await GetUser(login);
            ValidateLogin(login, user);
            await ValidateAppPermission(login, user);
            return await GetLoginResponseAsync(user, login.RemindMe);
        }

        public async Task<LoginResponseDto> ImpersonateAsync(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();
            return await GetLoginResponseAsync(user, false);
        }

        public async Task RequestPasswordRecoverAsync(string email, string tenantId)
        {
            var currentUser = await usersRepository.GetByMainEmailAsync(email, tenantId) ?? throw new UnauthorizedAccessException();
            currentUser.IsChangePasswordRequested = true;
            currentUser.ChangePasswordCodes.Add(
                 new ValidationCode
                 {
                     Code = CodeHelper.GenerateAlphanumericCode(usersApiSettings.ValidationCodesSize),
                     ExpirationDate = DateTime.UtcNow.AddMinutes(usersApiSettings.ValidationCodesDurationMinutes)
                 });
            await usersRepository.SaveAsync(currentUser);
            await SendEmail(currentUser);
        }

        public async Task PasswordRecoverAsync(RecoverPasswordDto recoverPassword)
        {
            var currentUser = await usersRepository.GetByRecoverPasswordCodeAsync(recoverPassword.Code, recoverPassword.TenantId) ?? throw new UnauthorizedAccessException();
            if (!currentUser.ChangePasswordCodes
                    .Any(c =>
                        c.Code.Equals(recoverPassword.Code, StringComparison.InvariantCultureIgnoreCase)
                        && c.ExpirationDate.CompareTo(DateTime.UtcNow) >= 0))
            {
                throw new UnauthorizedAccessException("Code");
            }
            var passwordValidation = PasswordValidation.Validate(recoverPassword.NewPassword, usersApiSettings.PasswordPattern);
            if (passwordValidation.Any())
            {
                throw new InvalidUserException(passwordValidation);
            }
            currentUser.PasswordHash = recoverPassword.NewPassword.HashMd5();
            currentUser.IsChangePasswordRequested = false;
            currentUser.ChangePasswordCodes.RemoveAll(c => c.Code.Equals(recoverPassword.Code, StringComparison.InvariantCultureIgnoreCase));
            _ = await usersRepository.SaveAsync(currentUser);
        }

        public async Task<LoginResponseDto> RefreshToken(string token, UserDto loggedUser)
        {
            var user = await ValidateRenewUser(loggedUser);
            var jwtToken = await GetJwtToken(token);
            if (jwtToken.Payload.TryGetValue("refresh_until", out var value)
                && DateTime.TryParse(value.ToString(), out var refreshUntil)
                && refreshUntil.CompareTo(DateTime.UtcNow) > 0)
            {
                return await GetLoginResponseAsync(user, true);
            }
            throw new UnauthorizedAccessException();
        }

        private async Task<User> ValidateRenewUser(UserDto loggedUser)
        {
            var user = await usersRepository.GetByIdAsync(loggedUser.Id) ?? throw new UnauthorizedAccessException("User not found");
            var currentUser = mapper.Map<UserDto>(user);
            if (currentUser == null
                || !currentUser.Permissions.All(r => loggedUser.Permissions.Any(x => x.Equals(r, StringComparison.InvariantCultureIgnoreCase))))
            {
                throw new UnauthorizedAccessException("User data changed");
            }
            return user;
        }

        private async Task<JwtSecurityToken> GetJwtToken(string token)
        {
            if (string.IsNullOrEmpty(usersApiSettings.JwtAuth?.SecretKey))
            {
                await SaveNewJwtConfig();
            }

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = usersApiSettings.JwtAuth?.Issuer,
                ValidAudience = usersApiSettings.JwtAuth?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(usersApiSettings.JwtAuth?.SecretKey ?? "")),
                ValidateIssuer = !string.IsNullOrEmpty(usersApiSettings.JwtAuth?.Issuer),
                ValidateAudience = !string.IsNullOrEmpty(usersApiSettings.JwtAuth?.Audience),
                ValidateLifetime = false
            };
            _ = handler.ValidateToken(token, parameters, out var jwtToken);
            return jwtToken as JwtSecurityToken ?? throw new UnauthorizedAccessException("Invalid token");
        }

        private async Task<LoginResponseDto> GetLoginResponseAsync(User user, bool remind, bool expired = false)
        {
            var response = new LoginResponseDto
            {
                User = OpenClearUser(user)
            };
            var claimsdata = new[]
            {
                 new  Claim("user", JsonSerializer.Serialize(await ClearUser(user), new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault})),
                 new  Claim("key", usersApiSettings?.JwtAuth?.ClientKey ?? string.Empty)
            };
            if (remind)
            {
                claimsdata = claimsdata.Append(new Claim("refresh_until", DateTime.UtcNow.AddDays(7).ToString())).ToArray();
            }
            else
            {
                claimsdata = claimsdata.Append(new Claim("refresh_until", DateTime.UtcNow.AddDays(1).ToString())).ToArray();
            }
            var expiry = DateTime.UtcNow.AddDays(1);
            if (expired)
            {
                expiry = DateTime.UtcNow.AddMinutes(-10);
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(usersApiSettings.JwtAuth?.SecretKey ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: usersApiSettings.JwtAuth?.Issuer, audience: usersApiSettings.JwtAuth?.Audience,
                expires: expiry, signingCredentials: credentials, claims: claimsdata);
            var tokenHandler = new JwtSecurityTokenHandler();
            response.Token = tokenHandler.WriteToken(token);
            return response;
        }

        private async Task<UserDto> ClearUser(User user)
        {
            var result = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                Surname = user.Surname,
                SocialName = user.SocialName,
                IsActive = user.IsActive,
                TenantId = user.TenantId,
                Roles = user.Roles,
                IsChangePasswordRequested = user.IsChangePasswordRequested,
                Emails = user.Emails.Where(e => e.IsMain).Select(e => mapper.Map<UserEmailDto>(e)).ToList(),
                NewPassword = null
            };
            var roles = await GetRoles(user);
            var tenant = await GetTenant(user);
            result.Permissions = roles.SelectMany(r => r.Permissions).Distinct().ToList();
            result.Tenant = new TenantDto { Id = tenant.Id, Name = tenant.Name, CNPJ = tenant.CNPJ, Features = tenant.Features };
            return result;
        }

        private UserDto OpenClearUser(User user)
        {
            var result = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                Surname = user.Surname,
                SocialName = user.SocialName,
                IsActive = user.IsActive,
                TenantId = user.TenantId,
                IsChangePasswordRequested = user.IsChangePasswordRequested,
                Emails = user.Emails.Where(e => e.IsMain).Select(e => mapper.Map<UserEmailDto>(e)).ToList(),
                Addresses = user.Addresses.Select(a => mapper.Map<AddressDto>(a)).ToList(),
                Documents = user.Documents.Select(d => mapper.Map<UserDocumentDto>(d)).ToList(),
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                NewPassword = null,
                Occupation = user.Occupation,
                Roles = user.Roles,
            };
            return result;
        }

        private async Task<TenantDto> GetTenant(User user)
        {
            return await tenantsService.GetByIdAsync(user.TenantId) ?? throw new NotFoundException("Tenant not found");
        }

        private void ValidateLogin(LoginDto login, User user)
        {
            if (!user.IsActive)
            {
                logger.Warning("Inactive user: {user}, Tenantid: {Tenantid}", user.Login, user.TenantId);
                throw new UnauthorizedAccessException();
            }
            if (!CheckPassword(user, login.PasswordHash.HashMd5()))
            {
                logger.Warning("Invalid password: {user}, Tenantid: {Tenantid}", user.Login, user.TenantId);
                throw new UnauthorizedAccessException();
            }
            if (!user.Emails.Exists(e => e.IsMain && e.IsValidated))
            {
                logger.Warning("Main email not validated: {user}, Tenantid: {Tenantid}", user.Login, user.TenantId);
                throw new NotValidatedEmailException("Email not validated");
            }
        }

        private static bool CheckPassword(User user, string password)
        {
            var result = user.PasswordHash.Equals(password, StringComparison.InvariantCultureIgnoreCase);
            if (!result && user.TempPassword != null && user.TempPassword.ExpirationDate >= DateTime.UtcNow)
            {
                result = user.TempPassword.PasswordHash.Equals(password, StringComparison.InvariantCultureIgnoreCase);
            }
            return result;
        }

        private async Task ValidateAppPermission(LoginDto login, User user)
        {
            var roles = await GetRoles(user);
            var permissions = roles.SelectMany(r => r.Permissions);
            var appSettings = usersApiSettings.AppsAccessSettings.Find(a => a.AppName.Equals(login.AppName, StringComparison.InvariantCultureIgnoreCase));
            if (appSettings == null || !appSettings.AppPermissions.Split(',').All(a => permissions.Any(p => p.Equals(a, StringComparison.InvariantCultureIgnoreCase))))
            {
                logger.Warning("Invalid app permissions for user {user}, Tenantid: {Tenantid}, app: {app}, permissions: {settings}, user permissions: {userPermissions}", user.Login, user.TenantId, login.AppName, appSettings?.AppPermissions, string.Join(",", permissions));
                throw new UnauthorizedAccessException("Invalid app permissions");
            }
        }

        private async Task<User> GetUser(LoginDto login)
        {
            var user = await usersRepository.GetByMainEmailAsync(login.Login, login.TenantId);
            if (user == null)
            {
                user = await usersRepository.GetByLoginAsync(login.Login, login.TenantId);
            }
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }
            return user;
        }

        private async Task<List<Role>> GetRoles(User user)
        {
            var result = new List<Role>();
            foreach (var roleName in user.Roles)
            {
                var role = await rolesRepository.GetByNameAsync(roleName, user.TenantId);
                if (role != null)
                {
                    result.Add(role);
                }
            }
            return result;
        }

        private async Task SendEmail(User user)
        {
            try
            {
                var emailValidation = new EmailSendDto
                {
                    TenantId = user.TenantId,
                    SendTo = new List<string> { user.MainEmail },
                    Fields = new Dictionary<string, string> {
                    { "CODE", user.ChangePasswordCodes.Last().Code },
                    { "NAME", user.Name },
                    { "LINK", $"{usersApiSettings.PasswordRecoverUrl}?tenantId={user.TenantId}&email={user.MainEmail}&user={user.Id}&code={user.ChangePasswordCodes.Last().Code}" }
                },
                    TemplateCode = usersApiSettings.PasswordRecoverTemplate,
                    ServiceCode = usersApiSettings.PasswordRecoverService
                };
                _ = emailsService.ComposeEmail(emailValidation);
            }
            catch (Exception ex)
            {
                logger.Error("Send recover password email failed: {message}", ex.Message, ex);
            }
        }

        private async Task SaveNewJwtConfig()
        {
            usersApiSettings.JwtAuth?.SecretKey = CodeHelper.GenerateAlphanumericCode(30).HashMd5();
            usersApiSettings.JwtAuth?.ClientKey = CodeHelper.GenerateAlphanumericCode(30).HashMd5();
            if (string.IsNullOrEmpty(usersApiSettings?.JwtAuth?.Issuer))
            {
                usersApiSettings!.JwtAuth!.Issuer = "onhive.com.br";
            }
            if (string.IsNullOrEmpty(usersApiSettings.JwtAuth.Audience))
            {
                usersApiSettings.JwtAuth.Audience = "onhive.com.br";
            }
            var authSettings = ServiceProviderFactory.ServiceProvider?.GetRequiredService<AuthSettings>() ?? new AuthSettings();
            authSettings.Secret = usersApiSettings.JwtAuth?.SecretKey;
            authSettings.Audience = usersApiSettings.JwtAuth?.Audience;
            authSettings.Issuer = usersApiSettings.JwtAuth?.Issuer;

            var currentUserSettings = await configurationService.GetByTypeAsync<UsersApiSettings>() ?? new ConfigItemDto { Key = nameof(UsersApiSettings) };
            var currentAuthSettings = await configurationService.GetByTypeAsync<AuthSettings>() ?? new ConfigItemDto { Key = nameof(AuthSettings) };

            currentAuthSettings.Value = authSettings;
            currentUserSettings.Value = usersApiSettings;

            await configurationService.SaveAsync(currentAuthSettings, null);
            await configurationService.SaveAsync(currentUserSettings, null);
        }
    }
}