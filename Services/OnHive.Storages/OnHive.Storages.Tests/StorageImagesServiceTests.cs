using AutoMapper;
using FluentAssertions;
using Moq;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Entities.Storages;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Storages.Domain.Abstractions.Repositories;
using OnHive.Storages.Domain.Mappers;
using OnHive.Storages.Domain.Models;
using OnHive.Storages.Services;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Events.Domain.Abstractions.Services;

namespace OnHive.Storages.Tests
{
    public class StorageImagesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IStorageImagesRepository> mockStoragesRepository;
        private readonly StoragesApiSettings storagesApiSettings;
        private readonly IMapper mapper;
        private readonly Mock<IEventRegister> mockEventRegister;

        public StorageImagesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockStoragesRepository = mockRepository.Create<IStorageImagesRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            storagesApiSettings = new StoragesApiSettings();
            storagesApiSettings.StoragesAdminPermission = "storages_admin";
        }

        [Fact]
        public async Task GetStorages_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<StorageImageFile>
            {
                new StorageImageFile
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockStoragesRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockStoragesRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetStorageByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<StorageImageFile>
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

            mockStoragesRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<StorageImageFile> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<StorageImageFileDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockStoragesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetStoragesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<StorageImageFileDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockStoragesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageImageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockStoragesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockStoragesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageImageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageImageFileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockStoragesRepository.Setup(r => r.SaveAsync(It.IsAny<StorageImageFile>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageImageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageImageFile>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageImageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageImageFileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId,
                ImageId = "test.png",
                Name = "Test",
            };

            mockStoragesRepository.Setup(r => r.SaveAsync(It.IsAny<StorageImageFile>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageImageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageImageFile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageImageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageImageFileDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId,
                ImageId = "test.png",
                Name = "Test",
            };

            mockStoragesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStoragesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageImageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageImageFile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStorageUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new StorageImageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageImageFileDto
            {
                Id = expected.Id,
                TenantId = "222222222222222",
                ImageId = "test.png",
                Name = "Test",
            };

            mockStoragesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStoragesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockStoragesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageImageFile>(), loggedUser!.User!.Id), Times.Never);
        }

        private StorageImagesService CreateService()
        {
            return new StorageImagesService(
                mockStoragesRepository.Object,
                storagesApiSettings,
                mapper,
                mockEventRegister.Object);
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
                    Roles = new List<string> { "admin", "staff" },
                    Permissions = new List<string> { "admin", "storages_admin" },
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