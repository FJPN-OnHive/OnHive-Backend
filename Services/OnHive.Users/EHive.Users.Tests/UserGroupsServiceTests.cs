using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Contracts.Common;
using EHive.Users.Domain.Abstractions.Repositories;
using EHive.Users.Domain.Mappers;
using EHive.Users.Domain.Models;
using EHive.Users.Services;

namespace EHive.Users.Tests
{
    public class UserGroupsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IUserGroupsRepository> mockUsersRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;

        public UserGroupsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockUsersRepository = mockRepository.Create<IUserGroupsRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            usersApiSettings = new UsersApiSettings();
            usersApiSettings.UserAdminPermission = "users_admin";
        }

        [Fact]
        public async Task GetUserGroups_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<UserGroup>
            {
                new UserGroup
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockUsersRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockUsersRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetUserGroupByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<UserGroup>
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

            mockUsersRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<UserGroup> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<UserGroupDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockUsersRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetUserGroupsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<UserGroupDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockUsersRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetUserGroup_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test",
                TenantId = loggedUser!.User!.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockUsersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveUserGroup_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserGroupDto
            {
                Id = string.Empty,
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<UserGroup>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserGroupDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<UserGroup>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateUserGroup_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserGroupDto
            {
                Id = string.Empty,
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            mockUsersRepository.Setup(r => r.SaveAsync(It.IsAny<UserGroup>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserGroupDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<UserGroup>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateUserGroup_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new UserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserGroupDto
            {
                Id = expected.Id,
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            mockUsersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockUsersRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<UserGroupDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockUsersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<UserGroup>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateUserGroupUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new UserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new UserGroupDto
            {
                Id = expected.Id,
                Name = "Test",
                TenantId = "222222222222222"
            };

            mockUsersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockUsersRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockUsersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockUsersRepository.Verify(r => r.SaveAsync(It.IsAny<UserGroup>(), loggedUser!.User!.Id), Times.Never);
        }

        private UserGroupsService CreateService()
        {
            return new UserGroupsService(
                mockUsersRepository.Object,
                usersApiSettings,
                mapper);
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