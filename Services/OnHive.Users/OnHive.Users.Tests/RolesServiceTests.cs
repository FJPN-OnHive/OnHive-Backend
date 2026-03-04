using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Users;
using OnHive.Users.Domain.Abstractions.Repositories;
using OnHive.Users.Domain.Mappers;
using OnHive.Users.Domain.Models;
using OnHive.Users.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace OnHive.Users.Tests
{
    public class RolesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IRolesRepository> mockRolesRepository;
        private readonly UsersApiSettings usersApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public RolesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockRolesRepository = mockRepository.Create<IRolesRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            usersApiSettings = new UsersApiSettings();
            usersApiSettings.UserAdminPermission = "users_admin";
        }

        [Fact]
        public async Task GetRoles_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Role>
            {
                new Role
                {
                    TenantId = user.User.TenantId
                }
            };

            mockRolesRepository.Setup(r => r.GetAllAsync(user.User.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.User.TenantId);
            mockRolesRepository.Verify(r => r.GetAllAsync(user.User.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetRoleByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Role>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
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
                        Operator = "eq",
                        Value = testUser.User.TenantId
                    }
                }
            };

            mockRolesRepository.Setup(r => r.GetByFilterAsync(filter, testUser.User.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Role> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<RoleDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockRolesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.User.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetRolesByFilter_NotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "eq",
                        Value = testUser.User.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<RoleDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockRolesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.User.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetRole_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.User.TenantId
            };

            mockRolesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.User.TenantId);
            mockRolesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveRole_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new RoleDto
            {
                Id = string.Empty,
                TenantId = testUser.User.TenantId
            };

            mockRolesRepository.Setup(r => r.SaveAsync(It.IsAny<Role>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RoleDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRolesRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateRole_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new RoleDto
            {
                Id = string.Empty,
                TenantId = testUser.User.TenantId
            };

            mockRolesRepository.Setup(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RoleDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRolesRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateRole_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new RoleDto
            {
                Id = expected.Id,
                TenantId = testUser.User.TenantId
            };

            mockRolesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockRolesRepository.Setup(r => r.SaveAsync(expected, testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RoleDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRolesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockRolesRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateRoleUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.User?.Permissions?.Clear();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.User.TenantId ?? string.Empty
            };

            var input = new RoleDto
            {
                Id = expected.Id,
                TenantId = testUser.User.TenantId
            };

            mockRolesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockRolesRepository.Setup(r => r.SaveAsync(expected, testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            try
            {
                _ = await service.UpdateAsync(input, testUser);
                Assert.Fail("Should throw UnauthorizedAccessException");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<UnauthorizedAccessException>();
            }

            // Assert
            mockRolesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockRolesRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id), Times.Never);
        }

        [Fact]
        public async Task PatchRole_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Role
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = $"{{\"id\":\"{expected.Id}\", \"tenantId\":\"{testUser.User.TenantId}\", \"name\":\"NewName\" }}";

            mockRolesRepository.Setup(r => r.GetByIdAsync(expected.Id))
               .ReturnsAsync(expected);

            mockRolesRepository.Setup(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.PatchAsync(JsonDocument.Parse(input), testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RoleDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRolesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockRolesRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), testUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task Migrate()
        {
            // Arrange
            var service = CreateService();
            mockRolesRepository.Setup(r => r.SaveAsync(It.IsAny<Role>(), It.IsAny<string>())).ReturnsAsync((Role u, string t) => u);

            // Act
            await service.Migrate(false);

            // Assert
            mockRolesRepository.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockRolesRepository.Verify(r => r.SaveAsync(It.Is<Role>(u => u.Name == "admin"), It.IsAny<string>()), Times.Once);
        }

        private RolesService CreateService()
        {
            return new RolesService(
                mockRolesRepository.Object,
                usersApiSettings,
                mapper,
                httpClient);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new LoggedUserDto(new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = new List<string> { "admin", "staff" },
                Permissions = new List<string> { "admin", "staff", "users_admin" },
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            }, "TOKEN");
        }
    }
}