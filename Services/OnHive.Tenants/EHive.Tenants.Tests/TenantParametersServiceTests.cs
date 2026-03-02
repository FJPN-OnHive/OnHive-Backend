using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Tenants;
using EHive.Tenants.Domain.Abstractions.Repositories;
using EHive.Tenants.Domain.Mappers;
using EHive.Tenants.Domain.Models;
using EHive.Tenants.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace EHive.Tenants.Tests
{
    public class TenantParametersServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ITenantParametersRepository> mockTenantParametersRepository;
        private readonly TenantsApiSettings tenantsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;

        public TenantParametersServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockTenantParametersRepository = mockRepository.Create<ITenantParametersRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            tenantsApiSettings = new TenantsApiSettings();
            tenantsApiSettings.TenantsAdminPermission = "tenants_admin";
        }

        [Fact]
        public async Task GetTenantParameters_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<TenantParameter>
            {
                new TenantParameter
                {
                    TenantId = user.TenantId
                }
            };

            mockTenantParametersRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockTenantParametersRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTenantParameterByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<TenantParameter>
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

            mockTenantParametersRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, false))
                .ReturnsAsync(new PaginatedResult<TenantParameter> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<TenantParameterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockTenantParametersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetTenantParametersByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<TenantParameterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockTenantParametersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetTenantParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockTenantParametersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockTenantParametersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task GetTenantParameterByGroup_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<TenantParameter>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user.TenantId,
                    Group = "A",
                    Key = "A"
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user.TenantId,
                    Group = "A",
                    Key = "B"
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user.TenantId,
                    Group = "A",
                    Key = "C"
                }
            };

            mockTenantParametersRepository.Setup(r => r.GetByGroup("A", user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByGroup("A", user.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(3);
            result?.Should().Satisfy(
                p => p.Key == "A" && p.Group == "A" && p.TenantId == user.TenantId,
                p => p.Key == "B" && p.Group == "A" && p.TenantId == user.TenantId,
                p => p.Key == "C" && p.Group == "A" && p.TenantId == user.TenantId);
            mockTenantParametersRepository.Verify(r => r.GetByGroup("A", user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTenantParameterByKey_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId,
                Group = "A",
                Key = "A"
            };

            mockTenantParametersRepository.Setup(r => r.GetByKey("A", "A", user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByKey("A", "A", user.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.TenantId.Should().Be(user.TenantId);
            result?.Key.Should().Be(expected.Key);
            result?.Group.Should().Be(expected.Group);
            mockTenantParametersRepository.Verify(r => r.GetByKey("A", "A", user.TenantId), Times.Once);
        }

        [Fact]
        public async Task SaveTenantParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantParameterDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTenantParametersRepository.Setup(r => r.SaveAsync(It.IsAny<TenantParameter>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantParameterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantParametersRepository.Verify(r => r.SaveAsync(It.IsAny<TenantParameter>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateTenantParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantParameterDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTenantParametersRepository.Setup(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantParameterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantParametersRepository.Verify(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantParameterDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTenantParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantParametersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantParameterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantParametersRepository.Verify(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTenantParameterUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.TenantId ?? string.Empty
            };

            var input = new TenantParameterDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTenantParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantParametersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockTenantParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockTenantParametersRepository.Verify(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id), Times.Never);
        }

        [Fact]
        public async Task PatchTenantParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new TenantParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TenantParameterDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId,
                Name = "Test"
            };

            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockTenantParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTenantParametersRepository.Setup(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TenantParameterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTenantParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTenantParametersRepository.Verify(r => r.SaveAsync(It.IsAny<TenantParameter>(), testUser.Id), Times.Once);
        }

        private TenantParametersService CreateService()
        {
            return new TenantParametersService(
                mockTenantParametersRepository.Object,
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