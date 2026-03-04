using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Courses.Domain.Mappers;
using OnHive.Courses.Domain.Models;
using OnHive.Courses.Services;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace OnHive.Courses.Tests
{
    public class LessonsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ILessonsRepository> mockLessonsRepository;
        private readonly Mock<IExamsService> mockExamService;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;

        public LessonsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockLessonsRepository = mockRepository.Create<ILessonsRepository>();
            mockExamService = mockRepository.Create<IExamsService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            coursesApiSettings = new CoursesApiSettings();
            coursesApiSettings.CoursesAdminPermission = "courses_admin";
        }

        [Fact]
        public async Task GetLessons_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Lesson>
            {
                new Lesson
                {
                    TenantId = user.TenantId
                }
            };

            mockLessonsRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockLessonsRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetLessonByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Lesson>
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

            mockLessonsRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Lesson> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<LessonDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockLessonsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetLessonsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<LessonDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockLessonsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetLesson_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Lesson
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockLessonsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockLessonsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveLesson_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Lesson
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new LessonDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockLessonsRepository.Setup(r => r.SaveAsync(It.IsAny<Lesson>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<LessonDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockLessonsRepository.Verify(r => r.SaveAsync(It.IsAny<Lesson>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateLesson_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Lesson
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new LessonDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockLessonsRepository.Setup(r => r.SaveAsync(It.IsAny<Lesson>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<LessonDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockLessonsRepository.Verify(r => r.SaveAsync(It.IsAny<Lesson>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateLesson_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Lesson
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new LessonDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockLessonsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockLessonsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<LessonDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockLessonsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockLessonsRepository.Verify(r => r.SaveAsync(It.IsAny<Lesson>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task PatchLesson_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Lesson
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId,
                Name = "teste1",
                Body = "teste",
                Code = "teste",
                Order = 1,
                Thumbnail = "teste",
                ImageUrl = "teste"
            };

            var input = @$"
                    {{
                        ""id"":""{expected.Id}"",
                        ""tenantId"":""{testUser.TenantId}"",
                        ""name"":""teste2""
                    }}";

            mockLessonsRepository.Setup(r => r.GetByIdAsync(expected.Id))
               .ReturnsAsync(expected);

            mockLessonsRepository.Setup(r => r.SaveAsync(It.IsAny<Lesson>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(JsonDocument.Parse(input), testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<LessonDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            result?.Name.Should().Be("teste2");
            result?.Code.Should().Be(expected.Code);
            mockLessonsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockLessonsRepository.Verify(r => r.SaveAsync(It.IsAny<Lesson>(), testUser.Id), Times.Once);
        }

        private LessonsService CreateService()
        {
            return new LessonsService(
                mockLessonsRepository.Object,
                coursesApiSettings,
                mapper,
                mockExamService.Object);
        }

        private UserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "courses_admin", "staff"],
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