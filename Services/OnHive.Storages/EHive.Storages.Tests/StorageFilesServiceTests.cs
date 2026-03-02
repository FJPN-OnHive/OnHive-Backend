using AutoMapper;
using FluentAssertions;
using Moq;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Storages;
using EHive.Core.Library.Contracts.Common;
using EHive.Storages.Domain.Abstractions.Repositories;
using EHive.Storages.Domain.Mappers;
using EHive.Storages.Domain.Models;
using EHive.Storages.Services;
using EHive.Core.Library.Contracts.Storages;
using EHive.Events.Domain.Abstractions.Services;

namespace EHive.Storages.Tests
{
    public class StorageFilesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IStorageFilesRepository> mockStoragesRepository;
        private readonly StoragesApiSettings storagesApiSettings;
        private readonly IMapper mapper;
        private readonly Mock<IEventRegister> mockEventRegister;

        public StorageFilesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockStoragesRepository = mockRepository.Create<IStorageFilesRepository>();
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

            var expected = new List<StorageFile>
            {
                new StorageFile
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

            var expected = new List<StorageFile>
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
                .ReturnsAsync(new PaginatedResult<StorageFile> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<StorageFileDto>>();
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
            result?.Should().BeOfType<PaginatedResult<StorageFileDto>>();
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

            var expected = new StorageFile
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

            var expected = new StorageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageFileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockStoragesRepository.Setup(r => r.SaveAsync(It.IsAny<StorageFile>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageFile>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageFileDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId,
                FileId = "test.pdf",
                Name = "Test",
                Type = ".pdf"
            };

            mockStoragesRepository.Setup(r => r.SaveAsync(It.IsAny<StorageFile>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageFile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStorage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new StorageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageFileDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId,
                FileId = "test.pdf",
                Name = "Test",
                Type = ".pdf"
            };

            mockStoragesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStoragesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StorageFileDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStoragesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageFile>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStorageUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new StorageFile
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new StorageFileDto
            {
                Id = expected.Id,
                TenantId = "222222222222222",
                FileId = "test.pdf",
                Name = "Test",
                Type = ".pdf"
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
            mockStoragesRepository.Verify(r => r.SaveAsync(It.IsAny<StorageFile>(), loggedUser!.User!.Id), Times.Never);
        }

        private StorageFilesService CreateService()
        {
            return new StorageFilesService(
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