using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
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
    public class TenantThemesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ITenantThemesRepository> mockTenantThemesRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;

        public TenantThemesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockTenantThemesRepository = mockRepository.Create<ITenantThemesRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            tenantsApiSettings = new TenantsApiSettings();
            tenantsApiSettings.TenantsAdminPermission = "tenants_admin";
        }

        [Fact]
        public async Task GetTenantThemes_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<TenantTheme>
            {
                new TenantTheme
                {
                    TenantId = user.TenantId
                }
            };

            mockTenantThemesRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockTenantThemesRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTenantThemesByDomain_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            var expected =  new TenantTheme()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId,
                   IsActive = true
                };

            mockTenantThemesRepository.Setup(r => r.GetByTenant(testUser.TenantId))
                .ReturnsAsync(expected);

            // Act
            var result = await service.GetByTenantId(testUser.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantThemeDto>();
            result?.Should().NotBeNull();
            mockTenantThemesRepository.Verify(r => r.GetByTenant(testUser.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTenantThemeByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<TenantTheme>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.TenantId
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

            mockTenantThemesRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<TenantTheme> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<TenantThemeDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockTenantThemesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetTenantThemesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<TenantThemeDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockTenantThemesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetTenantTheme_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockTenantThemesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockTenantThemesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveTenantTheme_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantThemeDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTenantThemesRepository.Setup(r => r.SaveAsync(It.IsAny<TenantTheme>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantThemeDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantThemesRepository.Verify(r => r.SaveAsync(It.IsAny<TenantTheme>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateTenantTheme_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantThemeDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTenantThemesRepository.Setup(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantThemeDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantThemesRepository.Verify(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantTheme_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantThemeDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTenantThemesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantThemesRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantThemeDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantThemesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantThemesRepository.Verify(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantThemeUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.TenantId ?? string.Empty
            };

            var input = new TenantThemeDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTenantThemesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantThemesRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockTenantThemesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockTenantThemesRepository.Verify(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id), Times.Never);
        }

        [Fact]
        public async Task PatchTenantTheme_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantTheme
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantThemeDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockTenantThemesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantThemesRepository.Setup(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.PatchAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantThemeDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantThemesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantThemesRepository.Verify(r => r.SaveAsync(It.IsAny<TenantTheme>(), testUser.Id), Times.Once);
        }

        private TenantThemesService CreateService()
        {
            return new TenantThemesService(
                mockTenantThemesRepository.Object,
                tenantsApiSettings,
                mapper);
        }

        private UserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Tesst.com", IsValidated = true, IsMain = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "tenants_admin"],
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