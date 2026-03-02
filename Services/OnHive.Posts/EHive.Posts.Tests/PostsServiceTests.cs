using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Posts;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Posts;
using EHive.Posts.Domain.Abstractions.Repositories;
using EHive.Posts.Domain.Mappers;
using EHive.Posts.Domain.Models;
using EHive.Posts.Services;
using EHive.Students.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;

namespace EHive.Posts.Tests
{
    public class PostsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IStudentsService> mockStudentsService;
        private readonly Mock<IPostsRepository> mockBlogsRepository;
        private readonly Mock<IPostBackupRepository> mockPostBackupRepository;
        private readonly PostsApiSettings blogsApiSettings;
        private readonly IMapper mapper;

        public PostsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockBlogsRepository = mockRepository.Create<IPostsRepository>();
            mockPostBackupRepository = mockRepository.Create<IPostBackupRepository>();
            mockStudentsService = mockRepository.Create<IStudentsService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            blogsApiSettings = new PostsApiSettings();
            blogsApiSettings.PostsAdminPermission = "posts_admin";
        }

        [Fact]
        public async Task GetPosts_Test()
        {
            // Arrange^.
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<BlogPost>
            {
                new BlogPost
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockBlogsRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockBlogsRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetPostByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<BlogPost>
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

            mockBlogsRepository.Setup(r => r.GetPublishedByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<BlogPost> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<BlogPostDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockBlogsRepository.Verify(r => r.GetPublishedByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetPostsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<BlogPostDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockBlogsRepository.Verify(r => r.GetPublishedByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetPost_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new BlogPost
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockBlogsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockBlogsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SavePost_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new BlogPost
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new BlogPostDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockBlogsRepository.Setup(r => r.SaveAsync(It.IsAny<BlogPost>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<BlogPostDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockBlogsRepository.Verify(r => r.SaveAsync(It.IsAny<BlogPost>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreatePost_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new BlogPost
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new BlogPostDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId,
                Title = "ABC",
                Body = "ABC"
            };

            mockBlogsRepository.Setup(r => r.SaveAsync(It.IsAny<BlogPost>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<BlogPostDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockBlogsRepository.Verify(r => r.SaveAsync(It.IsAny<BlogPost>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdatePost_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new BlogPost
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new BlogPostDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId,
                Title = "ABC",
                Body = "ABC"
            };

            mockBlogsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockBlogsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<BlogPostDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockBlogsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockBlogsRepository.Verify(r => r.SaveAsync(It.IsAny<BlogPost>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdatePostUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new BlogPost
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new BlogPostDto
            {
                Id = expected.Id,
                TenantId = "222222222222222",
                Title = "ABC",
                Body = "ABC"
            };

            mockBlogsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockBlogsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockBlogsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockBlogsRepository.Verify(r => r.SaveAsync(It.IsAny<BlogPost>(), loggedUser!.User!.Id), Times.Never);
        }

        private PostsService CreateService()
        {
            return new PostsService(
                mockBlogsRepository.Object,
                mockPostBackupRepository.Object,
                blogsApiSettings,
                mapper,
                mockStudentsService.Object);
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
                    Permissions = new List<string> { "admin", "blogs_admin" },
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