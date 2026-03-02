using AutoMapper;
using EHive.Configuration.Domain.Abstractions.Services;
using EHive.Core.Library.Contracts.Emails;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.SystemParameters;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Extensions;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.SystemParameters.Domain.Abstractions.Repositories;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Mappers;
using EHive.Users.Domain.Models;
using EHive.Users.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;

namespace EHive.Users.Tests
{
    public class LoginServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<IRolesRepository> mockRolesRepository;
        private Mock<ITenantsService> mockTenantsService;
        private Mock<IConfigurationService> mockConfigrationService;
        private Mock<ISystemParametersRepository> mockSystemParametersService;
        private UsersApiSettings usersApiSettings;
        private IMapper mapper;
        private Mock<IEmailsService> mockEmailService;

        public LoginServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUsersRepository = mockRepository.Create<IUsersRepository>();
            mockRolesRepository = mockRepository.Create<IRolesRepository>();
            mockTenantsService = mockRepository.Create<ITenantsService>();
            mockSystemParametersService = mockRepository.Create<ISystemParametersRepository>();
            mockConfigrationService = mockRepository.Create<IConfigurationService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockEmailService = mockRepository.Create<IEmailsService>();
            usersApiSettings = new UsersApiSettings
            {
                JwtAuth = new JwtAuth
                {
                    SecretKey = "123456789".HashMd5(),
                    Issuer = "test",
                    Audience = "test",
                    RenewTimeHours = 48,
                    ExpirationTimeMinutes = 90
                }
            };
        }

        [Fact]
        public async Task LoginBasicExpiredAsync_ByLogin()
        {
            // Arrange
            var service = CreateService();
            var login = new LoginDto
            {
                Login = "test",
                TenantId = "11111111111111",
                RemindMe = true,
                PasswordHash = "se1@n1AS",
                AppName = "LMS",
                Redirect = "callback"
            };
            var expected = new User
            {
                Login = "test",
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                Emails = new List<UserEmail> { new UserEmail { Email = "test@test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = new List<string> { "admin" }
            };
            var tenant = new TenantDto
            {
                Id = "11111111111111",
                Features = new List<string>()
            };
            var role = new Role
            {
                Id = "admin",
                Name = "admin",
                Permissions = new List<string> { "lms_access" }
            };

            mockRolesRepository.Setup(r => r.GetByNameAsync(role.Name, login.TenantId)).ReturnsAsync(role);
            mockUsersRepository.Setup(r => r.GetByLoginAsync(login.Login, login.TenantId)).ReturnsAsync(expected);
            mockTenantsService.Setup(t => t.GetByIdAsync(login.TenantId)).ReturnsAsync(tenant);

            // Act
            var result = await service.LoginBasicExpiredAsync(login);

            // Assert
            result.Should().NotBeNull();
            result.User.Login.Should().Be(login.Login);
            result.Token.Should().NotBeEmpty();
            mockUsersRepository.Verify(r => r.GetByLoginAsync(login.Login, login.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(login.Login, login.TenantId), Times.Never);
        }

        [Fact]
        public async Task LoginBasicExpiredAsync_ByEmail()
        {
            // Arrange
            var service = CreateService();
            var login = new LoginDto
            {
                Login = "test@test.com",
                TenantId = "11111111111111",
                RemindMe = true,
                PasswordHash = "se1@n1AS",
                AppName = "LMS",
                Redirect = "callback"
            };
            var expected = new User
            {
                Login = "test_other",
                Emails = new List<UserEmail> { new UserEmail { Email = login.Login, IsMain = true, IsValidated = true } },
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                IsActive = true,
                Roles = new List<string> { "admin" }
            };
            var tenant = new TenantDto
            {
                Id = "11111111111111",
                Features = new List<string>()
            };
            var role = new Role
            {
                Id = "admin",
                Name = "admin",
                Permissions = new List<string> { "lms_access" }
            };

            mockRolesRepository.Setup(r => r.GetByNameAsync(role.Name, login.TenantId)).ReturnsAsync(role);
            mockUsersRepository.Setup(r => r.GetByLoginAsync(login.Login, login.TenantId)).ReturnsAsync(() => null);
            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(login.Login, login.TenantId)).ReturnsAsync(expected);
            mockTenantsService.Setup(t => t.GetByIdAsync(login.TenantId)).ReturnsAsync(tenant);

            // Act
            var result = await service.LoginBasicExpiredAsync(login);

            // Assert
            result.Should().NotBeNull();
            result.User.MainEmail.Should().Be(login.Login);
            result.Token.Should().NotBeEmpty();
            mockUsersRepository.Verify(r => r.GetByLoginAsync(login.Login, login.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(login.Login, login.TenantId), Times.Once);
        }

        [Fact]
        public async Task LoginAsync()
        {
            // Arrange
            var service = CreateService();
            var login = new LoginDto
            {
                Login = "test",
                TenantId = "11111111111111",
                RemindMe = true,
                PasswordHash = "se1@n1AS",
                AppName = "LMS",
                Redirect = "callback"
            };
            var expected = new User
            {
                Login = "test_other",
                Emails = new List<UserEmail> { new UserEmail { Email = "test@teste.com", IsMain = true, IsValidated = true } },
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                IsActive = true,
                Roles = new List<string> { "admin" }
            };
            var tenant = new TenantDto
            {
                Id = "11111111111111",
                Features = new List<string>()
            };
            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = "admin",
                Permissions = new List<string>
                    {
                        "lms_access"
                    }
            };
            var appInfo = new SystemParameter
            {
                Id = "LMS_DOMAIN",
                Value = "http://lms.localhost.com"
            };

            var appVersion = new SystemParameter
            {
                Id = "SYSTEM_VERSION",
                Value = "1.0"
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(login.Login, login.TenantId)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);
            mockRolesRepository.Setup(r => r.GetByNameAsync("admin", login.TenantId)).ReturnsAsync(role);

            mockTenantsService.Setup(t => t.GetByIdAsync(login.TenantId)).ReturnsAsync(tenant);

            mockSystemParametersService.Setup(t => t.GetByIdAsync("LMS_DOMAIN")).ReturnsAsync(appInfo);

            mockSystemParametersService.Setup(t => t.GetByIdAsync("SYSTEM_VERSION")).ReturnsAsync(appVersion);

            // Act
            var result = await service.LoginAsync(login);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeEmpty();
        }

        [Fact]
        public async Task LoginAsync_UnauthorizedApp()
        {
            // Arrange
            var service = CreateService();
            var login = new LoginDto
            {
                Login = "test",
                TenantId = "11111111111111",
                RemindMe = true,
                PasswordHash = "se1@n1AS".HashMd5(),
                AppName = "ADMIN",
                Redirect = "callback"
            };
            var expected = new User
            {
                Login = "test_other",
                Emails = new List<UserEmail> { new UserEmail { Email = "test@teste.com", IsMain = true, IsValidated = true } },
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                IsActive = true,
                Roles = new List<string> { "admin" }
            };
            var tenant = new TenantDto
            {
                Id = "11111111111111",
                Features = new List<string>()
            };
            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = "admin",
                Permissions = new List<string>
                    {
                        "lms_access"
                    }
            };
            var appInfo = new SystemParameter
            {
                Id = "LMS_DOMAIN",
                Value = "http://lms.localhost.com"
            };
            var appVersion = new SystemParameter
            {
                Id = "SYSTEM_VERSION",
                Value = "1.0"
            };

            mockUsersRepository.Setup(r => r.GetByLoginAsync(login.Login, login.TenantId)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);
            mockRolesRepository.Setup(r => r.GetByNameAsync("admin", login.TenantId)).ReturnsAsync(role);

            mockTenantsService.Setup(t => t.GetByIdAsync(login.TenantId)).ReturnsAsync(tenant);

            mockSystemParametersService.Setup(t => t.GetByIdAsync("LMS_DOMAIN")).ReturnsAsync(appInfo);

            mockSystemParametersService.Setup(t => t.GetByIdAsync("SYSTEM_VERSION")).ReturnsAsync(appVersion);

            // Act
            try
            {
                var result = await service.LoginAsync(login);
                Assert.Fail("Must throw exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<UnauthorizedAccessException>();
            }

            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RequestPasswordRecoverAsync()
        {
            // Arrange
            var service = CreateService();
            string email = "test@test.com";
            string tenantId = "11111111111111";
            var expected = new User
            {
                Login = "test_other",
                Emails = new List<UserEmail> { new UserEmail { Email = "test@teste.com", IsMain = true, IsValidated = true } },
                TenantId = tenantId,
                PasswordHash = "se1@n1AS".HashMd5(),
                IsActive = true
            };

            mockUsersRepository.Setup(r => r.GetByMainEmailAsync(email, expected.TenantId)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);
            mockEmailService.Setup(e => e.ComposeEmail(It.IsAny<EmailSendDto>()));

            // Act
            await service.RequestPasswordRecoverAsync(email, tenantId);

            // Assert
            mockUsersRepository.Verify(r => r.GetByMainEmailAsync(email, expected.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            mockEmailService.Verify(r => r.ComposeEmail(
                It.Is<EmailSendDto?>(u => u != null &&
                                        u.SendTo.Contains(expected.MainEmail) &&
                                        u.TemplateCode == usersApiSettings.PasswordRecoverTemplate)),
                                     Times.Once);
        }

        [Fact]
        public async Task PasswordRecoverAsync()
        {
            // Arrange
            var service = this.CreateService();
            var recoverPassword = new RecoverPasswordDto
            {
                TenantId = "11111111111111",
                Code = "ABC123",
                NewPassword = "se1@n1AS"
            };
            var expected = new User
            {
                Login = "test",
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                Emails = new List<UserEmail>
                {
                    new UserEmail
                    {
                        Email = "test@test.com",
                        IsMain = true,
                        IsValidated = false
                    }
                },
                IsActive = true,
                ChangePasswordCodes = new List<ValidationCode>
                {
                    new ValidationCode
                    {
                        Code = "ABC123",
                        ExpirationDate = DateTime.UtcNow.AddDays(1),
                    }
                }
            };
            mockUsersRepository.Setup(r => r.GetByRecoverPasswordCodeAsync(recoverPassword.Code, recoverPassword.TenantId)).ReturnsAsync(expected);
            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User u, string t) => u);

            // Act
            await service.PasswordRecoverAsync(recoverPassword);

            // Assert
            mockUsersRepository.Verify(r => r.GetByRecoverPasswordCodeAsync(recoverPassword.Code, recoverPassword.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.Is<User>(u => u.Id == expected.Id && !u.ChangePasswordCodes.Any()), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_Valid()
        {
            // Arrange
            var service = this.CreateService();
            var login = new LoginDto
            {
                Login = "test",
                TenantId = "11111111111111",
                RemindMe = true,
                PasswordHash = "se1@n1AS",
                AppName = "lms"
            };
            var expected = new User
            {
                Login = "test",
                TenantId = "11111111111111",
                PasswordHash = "se1@n1AS".HashMd5(),
                Emails = new List<UserEmail> { new UserEmail { Email = "test@test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = new List<string> { "admin" }
            };
            var tenant = new TenantDto
            {
                Id = "11111111111111",
                Features = new List<string>()
            };
            var role = new Role
            {
                Id = "admin",
                Name = "admin",
                Permissions = new List<string> { "lms_access" }
            };

            mockRolesRepository.Setup(r => r.GetByNameAsync(role.Name, login.TenantId)).ReturnsAsync(role);
            mockUsersRepository.Setup(r => r.GetByLoginAsync(login.Login, login.TenantId)).ReturnsAsync(expected);
            mockTenantsService.Setup(t => t.GetByIdAsync(login.TenantId)).ReturnsAsync(tenant);
            var loginResult = await service.LoginAsync(login);
            mockUsersRepository.Setup(r => r.GetByIdAsync(loginResult.User.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.RefreshToken(loginResult.Token, loginResult.User);

            // Assert
            result.Should().NotBeNull();
            result.User.Id.Should().Be(loginResult.User.Id);
            mockUsersRepository.Verify(r => r.GetByLoginAsync(loginResult.User.Login, loginResult.User.TenantId), Times.Once);
            mockUsersRepository.Verify(r => r.GetByIdAsync(loginResult.User.Id), Times.Once);
        }

        private LoginService CreateService()
        {
            return new LoginService(
                mockUsersRepository.Object,
                mockRolesRepository.Object,
                usersApiSettings,
                mapper,
                mockEmailService.Object,
                mockTenantsService.Object,
                mockConfigrationService.Object);
        }
    }
}