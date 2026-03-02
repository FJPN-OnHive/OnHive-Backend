using AutoMapper;
using FluentAssertions;
using Moq;
using EHive.Core.Library.Contracts.Dict;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Dict;
using EHive.Core.Library.Contracts.Common;
using EHive.Dict.Domain.Abstractions.Repositories;
using EHive.Dict.Domain.Mappers;
using EHive.Dict.Domain.Models;
using EHive.Dict.Services;

namespace EHive.Dict.Tests
{
    public class DictServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IDictRepository> mockDictRepository;
        private readonly DictApiSettings dictApiSettings;
        private readonly IMapper mapper;

        public DictServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockDictRepository = mockRepository.Create<IDictRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            dictApiSettings = new DictApiSettings();
            dictApiSettings.DictAdminPermission = "dict_admin";
        }

        [Fact]
        public async Task GetValuess_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<ValueRegistry>
            {
                new ValueRegistry
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockDictRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockDictRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetValuesByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<ValueRegistry>
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

            mockDictRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId))
                .ReturnsAsync(new PaginatedResult<ValueRegistry> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<ValueRegistryDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockDictRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetValuessByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<ValueRegistryDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockDictRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetValues_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new ValueRegistry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockDictRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockDictRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveValues_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new ValueRegistry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new ValueRegistryDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockDictRepository.Setup(r => r.SaveAsync(It.IsAny<ValueRegistry>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ValueRegistryDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDictRepository.Verify(r => r.SaveAsync(It.IsAny<ValueRegistry>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateValues_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new ValueRegistry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new ValueRegistryDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockDictRepository.Setup(r => r.SaveAsync(It.IsAny<ValueRegistry>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ValueRegistryDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDictRepository.Verify(r => r.SaveAsync(It.IsAny<ValueRegistry>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateValues_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new ValueRegistry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new ValueRegistryDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockDictRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockDictRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ValueRegistryDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDictRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockDictRepository.Verify(r => r.SaveAsync(It.IsAny<ValueRegistry>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateValuesUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new ValueRegistry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new ValueRegistryDto
            {
                Id = expected.Id,
                TenantId = "222222222222222"
            };

            mockDictRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockDictRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockDictRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockDictRepository.Verify(r => r.SaveAsync(It.IsAny<ValueRegistry>(), loggedUser!.User!.Id), Times.Never);
        }

        private DictService CreateService()
        {
            return new DictService(
                mockDictRepository.Object,
                dictApiSettings,
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
                    Permissions = new List<string> { "admin", "dict_admin" },
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