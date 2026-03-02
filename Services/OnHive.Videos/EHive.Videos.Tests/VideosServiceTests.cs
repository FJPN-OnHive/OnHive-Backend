using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using EHive.Core.Library.Contracts.Videos;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Videos;
using EHive.Core.Library.Contracts.Common;
using EHive.Videos.Domain.Abstractions.Repositories;
using EHive.Videos.Domain.Mappers;
using EHive.Videos.Domain.Models;
using EHive.Videos.Services;
using EHive.Events.Domain.Abstractions.Services;

namespace EHive.Videos.Tests
{
    public class VideosServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IVideosRepository> mockVideosRepository;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly VideosApiSettings videosApiSettings;
        private readonly IMapper mapper;

        public VideosServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockVideosRepository = mockRepository.Create<IVideosRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();

            videosApiSettings = new VideosApiSettings();
            videosApiSettings.VideosAdminPermission = "videos_admin";
        }

        [Fact]
        public async Task GetVideos_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Video>
            {
                new Video
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockVideosRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockVideosRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetVideoByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Video>
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

            mockVideosRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Video> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<VideoDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockVideosRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetVideosByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<VideoDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockVideosRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetVideo_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Video
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockVideosRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockVideosRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveVideo_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Video
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new VideoDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockVideosRepository.Setup(r => r.SaveAsync(It.IsAny<Video>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<VideoDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockVideosRepository.Verify(r => r.SaveAsync(It.IsAny<Video>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateVideo_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Video
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new VideoDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockVideosRepository.Setup(r => r.SaveAsync(It.IsAny<Video>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<VideoDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockVideosRepository.Verify(r => r.SaveAsync(It.IsAny<Video>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateVideo_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Video
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new VideoDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockVideosRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockVideosRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<VideoDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockVideosRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockVideosRepository.Verify(r => r.SaveAsync(It.IsAny<Video>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateVideoUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new Video
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new VideoDto
            {
                Id = expected.Id,
                TenantId = "22222222222222"
            };

            mockVideosRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockVideosRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockVideosRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockVideosRepository.Verify(r => r.SaveAsync(It.IsAny<Video>(), loggedUser!.User!.Id), Times.Never);
        }

        private VideosService CreateService()
        {
            return new VideosService(
                mockVideosRepository.Object,
                videosApiSettings,
                mapper,
                mockEventRegister.Object
                );
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
                    Permissions = new List<string> { "admin", "videos_admin", "staff" },
                    Roles = new List<string> { "admin", "staff" },
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