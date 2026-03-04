using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
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
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using System.Text.Json;

namespace OnHive.Courses.Tests
{
    public class CoursesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ICoursesRepository> mockCoursesRepository;
        private readonly Mock<IDisciplineService> mockDisciplinesService;
        private readonly Mock<IStudentsService> mockStudentsService;
        private readonly Mock<IProductsService> mockProductsService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;

        public CoursesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockCoursesRepository = mockRepository.Create<ICoursesRepository>();
            mockDisciplinesService = mockRepository.Create<IDisciplineService>();
            mockStudentsService = mockRepository.Create<IStudentsService>();
            mockProductsService = mockRepository.Create<IProductsService>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(h => h.ProductsService).Returns(mockProductsService.Object);
            mockServicesHub.SetupGet(h => h.StudentsService).Returns(mockStudentsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            coursesApiSettings = new CoursesApiSettings();
            coursesApiSettings.CoursesAdminPermission = "courses_admin";
        }

        [Fact]
        public async Task GetCourses_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Course>
            {
                new Course
                {
                    TenantId = user.TenantId
                }
            };

            mockCoursesRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockCoursesRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCourseByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Course>
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
                        Operator = "EQ",
                        Value = testUser.TenantId
                    }
                }
            };

            mockCoursesRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Course> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CourseDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockCoursesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCoursesByFilter_NotFound_Test()
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
                        Operator = "EQ",
                        Value = testUser.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CourseDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockCoursesRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Course
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockCoursesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockCoursesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Course
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CourseDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId,
                Code = "123456",
                Name = "Test",
                Thumbnail = "test",
                ImageUrl = "test"
            };

            mockCoursesRepository.Setup(r => r.SaveAsync(It.IsAny<Course>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CourseDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCoursesRepository.Verify(r => r.SaveAsync(It.IsAny<Course>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Course
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CourseDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId,
                Code = "123456",
                Name = "Test",
                Thumbnail = "test",
                ImageUrl = "test"
            };

            mockCoursesRepository.Setup(r => r.SaveAsync(It.IsAny<Course>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CourseDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCoursesRepository.Verify(r => r.SaveAsync(It.IsAny<Course>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Course
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new CourseDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId,
                Code = "123456",
                Name = "Test",
                Thumbnail = "test",
                ImageUrl = "test"
            };

            mockCoursesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCoursesRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CourseDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCoursesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCoursesRepository.Verify(r => r.SaveAsync(It.IsAny<Course>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task PatchCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Course
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId,
                Name = "teste1",
                Body = "teste",
                Code = "teste",
                Thumbnail = "teste",
                ImageUrl = "teste"
            };

            var input = $@"
            {{
                ""id"": ""{expected.Id}"",
                ""tenantId"": ""{testUser.TenantId}"",
                ""name"": ""teste2""
            }}";

            mockCoursesRepository.Setup(r => r.GetByIdAsync(expected.Id))
               .ReturnsAsync(expected);

            mockCoursesRepository.Setup(r => r.SaveAsync(It.IsAny<Course>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(JsonDocument.Parse(input), testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CourseDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            result?.Name.Should().Be("teste2");
            mockCoursesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockCoursesRepository.Verify(r => r.SaveAsync(It.IsAny<Course>(), testUser.Id), Times.Once);
        }

        private CoursesService CreateService()
        {
            return new CoursesService(
                mockCoursesRepository.Object,
                mockDisciplinesService.Object,
                coursesApiSettings,
                mapper,
                mockEventRegister.Object,
                mockServicesHub.Object);
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