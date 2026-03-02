using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Tenants;

using EHive.Core.Library.Contracts.Users;

using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Users;

using EHive.Core.Library.Contracts.Users;

using EHive.Core.Library.Contracts.Common;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Mappers;
using EHive.Users.Domain.Models;
using EHive.Users.Services;
using System.Text.Json;

namespace EHive.Users.Tests
{
    public class UserProfilesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IUserProfilesRepository> mockUserProfilesRepository;
        private readonly Mock<IUsersRepository> mockUsersRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public UserProfilesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockUserProfilesRepository = mockRepository.Create<IUserProfilesRepository>();
            mockUsersRepository = mockRepository.Create<IUsersRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            usersApiSettings = new UsersApiSettings();
            usersApiSettings.UserAdminPermission = "users_admin";
        }

        [Fact]
        public async Task GetUserProfiles_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<UserProfile>
            {
                new UserProfile
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockUserProfilesRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockUserProfilesRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetUserProfileByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<UserProfile>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                }
            };

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = loggedUser!.User!.TenantId
                    }
                }
            };

            mockUserProfilesRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true))
                .ReturnsAsync(new PaginatedResult<UserProfile> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<UserProfileDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockUserProfilesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetUserProfilesByFilter_NotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = loggedUser!.User!.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<UserProfileDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockUserProfilesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetUserProfile_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = loggedUser!.User!.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockUserProfilesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockUserProfilesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveUserProfile_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserProfileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockUserProfilesRepository.Setup(r => r.SaveAsync(It.IsAny<UserProfile>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserProfileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUserProfilesRepository.Verify(r => r.SaveAsync(It.IsAny<UserProfile>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateUserProfile_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserProfileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockUserProfilesRepository.Setup(r => r.SaveAsync(It.IsAny<UserProfile>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserProfileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUserProfilesRepository.Verify(r => r.SaveAsync(It.IsAny<UserProfile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfile_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserProfileDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockUserProfilesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockUserProfilesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserProfileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUserProfilesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockUserProfilesRepository.Verify(r => r.SaveAsync(It.IsAny<UserProfile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserProfileDto
            {
                Id = expected.Id,
                TenantId = "222222222222222"
            };

            mockUserProfilesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockUserProfilesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            try
            {
                _ = await service.UpdateAsync(input, loggedUser);
                Assert.Fail("Should throw UnauthorizedAccessException");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<UnauthorizedAccessException>();
            }

            // Assert
            mockUserProfilesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockUserProfilesRepository.Verify(r => r.SaveAsync(It.IsAny<UserProfile>(), loggedUser!.User!.Id), Times.Never);
        }

        private UserProfilesService CreateService()
        {
            return new UserProfilesService(
                mockUserProfilesRepository.Object,
                mockUsersRepository.Object,
                usersApiSettings,
                mapper,
                httpClient);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new LoggedUserDto(
                new UserDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test",
                    Login = "Test",
                    Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                    IsActive = true,
                    Permissions = new List<string> { "admin", "users_admin" },
                    Roles = { "admin", "staff" },
                    TenantId = tenantId,
                    Tenant = new TenantDto
                    {
                        Id = tenantId,
                        Domain = "TestCo",
                        Email = "Test@TestCo.com",
                        Name = "TestCo",
                        Features = new List<string> { "homolog" }
                    },
                },
                "TOKEN"
            );
        }
    }
}