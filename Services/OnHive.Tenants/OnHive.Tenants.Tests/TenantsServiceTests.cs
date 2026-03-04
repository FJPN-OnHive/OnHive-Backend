using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Tenants.Domain.Abstractions.Repositories;
using OnHive.Tenants.Domain.Mappers;
using OnHive.Tenants.Domain.Models;
using OnHive.Tenants.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace OnHive.Tenants.Tests
{
    public class TenantsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ITenantsRepository> mockTenantsRepository;
        private readonly Mock<IFeaturesRepository> mockFeaturesRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;

        public TenantsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockTenantsRepository = mockRepository.Create<ITenantsRepository>();
            mockFeaturesRepository = mockRepository.Create<IFeaturesRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            tenantsApiSettings = new TenantsApiSettings();
            tenantsApiSettings.TenantsAdminPermission = "tenants_admin";
        }

        [Fact]
        public async Task GetTenants_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Tenant>
            {
                new Tenant
                {
                    Id = user.TenantId
                }
            };

            mockTenantsRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().Id.Should().Be(user.TenantId);
            mockTenantsRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTenant_Test()
        {
            // Arrange
            var service = CreateService();

            var expected = new Tenant
            {
                Id = Guid.NewGuid().ToString()
            };

            mockTenantsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            mockTenantsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task GetTenantByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Tenant>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId,
                   Name = "Name1",
                   Email = "Email1"
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId,
                   Name = "Name2",
                   Email = "Email2"
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId,
                   Name = "Name3",
                   Email = "Email3"
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
                        Value = testUser.TenantId
                    }
                }
            };

            mockTenantsRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Tenant> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<TenantDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockTenantsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetTenantByFilter_NotFound_Test()
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
                        Operator = "==",
                        Value = testUser.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<TenantDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockTenantsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task CreateTenant_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Tenant
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId,
                Name = "Name1",
                Email = "Email1"
            };

            var input = new TenantDto
            {
                Id = string.Empty,
                Name = "Name1",
                Email = "Email1"
            };

            mockTenantsRepository.Setup(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantDto>();
            result?.Name.Should().Be(expected.Name);
            mockTenantsRepository.Verify(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenant_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Tenant
            {
                Id = testUser.TenantId,
                TenantId = testUser.TenantId,
                Name = "Name1",
                Email = "Email1"
            };

            var input = new TenantDto
            {
                Id = expected.Id,
                Name = "Name1",
                Email = "Email1"
            };

            mockTenantsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantDto>();
            result?.Name.Should().Be(expected.Name);
            mockTenantsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantsRepository.Verify(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new Tenant
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.TenantId ?? string.Empty,
                Name = "Name1",
                Email = "Email1"
            };

            var input = new TenantDto
            {
                Id = expected.Id,
                Name = "Name1",
                Email = "Email1"
            };

            mockTenantsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockTenantsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockTenantsRepository.Verify(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id), Times.Never);
        }

        [Fact]
        public async Task PatchTenant_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Tenant
            {
                Id = testUser.TenantId,
                TenantId = testUser.TenantId,
                Name = "Name1",
                Email = "Email1"
            };

            var input = new TenantDto
            {
                Id = expected.Id,
                Name = "Test"
            };

            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockTenantsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantsRepository.Setup(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantDto>();
            result?.Name.Should().Be(expected.Name);
            mockTenantsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantsRepository.Verify(r => r.SaveAsync(It.IsAny<Tenant>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task Migration_Test()
        {
            // Arrange
            var service = CreateService();

            mockTenantsRepository.Setup(r => r.SaveAsync(It.IsAny<Tenant>(), It.IsAny<string>())).ReturnsAsync((Tenant t, string id) => t);

            // Act
            await service.Migrate(false);

            // Assert
            mockTenantsRepository.Verify(r => r.GetByIdAsync("11111111111111"), Times.Once);
            mockTenantsRepository.Verify(r => r.SaveAsync(It.IsAny<Tenant>(), It.IsAny<string>()), Times.Once);
        }

        private TenantsService CreateService()
        {
            return new TenantsService(
                mockTenantsRepository.Object,
                tenantsApiSettings,
                mockFeaturesRepository.Object,
                mapper);
        }

        private UserDto GetTestUser()
        {
            var tenantId = "1111111111111";
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsValidated = true, IsMain = true } },
                IsActive = true,

                Roles = ["admin", "staff"],
                Permissions = ["admin", "tenants_admin", "tenant_create", "tenants_update", "tenants_read", "staff"],
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            };
        }
    }
}