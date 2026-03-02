using AutoMapper;
using EHive.Catalog.Domain.Abstractions.Services;
using EHive.Certificates.Domain.Abstractions.Services;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Courses;
using EHive.Core.Library.Entities.Events;
using EHive.Core.Library.Entities.Students;
using EHive.Courses.Domain.Abstractions.Services;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Storages.Domain.Abstractions.Services;
using EHive.Students.Domain.Abstractions.Repositories;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Students.Domain.Mappers;
using EHive.Students.Domain.Models;
using EHive.Students.Services;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Videos.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace EHive.Students.Tests
{
    public class StudentsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IStudentsRepository> mockStudentsRepository;
        private readonly Mock<IStudentReportsRepository> mockStudentReportsRepository;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly Mock<IProductsService> mockProductsService;
        private readonly Mock<ICoursesService> mockCoursesService;
        private readonly Mock<IUsersService> mockUsersService;
        private readonly Mock<ICertificatesService> mockCertificatesService;
        private readonly Mock<IStudentActivitiesService> mockStudentActivitiesService;
        private readonly Mock<IExamsService> mockExamsService;
        private readonly Mock<IVideosService> mockVideosService;
        private readonly Mock<IStorageFilesService> mockStorageFilesService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly StudentsApiSettings studentsApiSettings;
        private readonly IMapper mapper;

        public StudentsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockStudentsRepository = mockRepository.Create<IStudentsRepository>();
            mockStudentReportsRepository = mockRepository.Create<IStudentReportsRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();

            mockStudentActivitiesService = mockRepository.Create<IStudentActivitiesService>();
            mockCertificatesService = mockRepository.Create<ICertificatesService>();
            mockCoursesService = mockRepository.Create<ICoursesService>();
            mockUsersService = mockRepository.Create<IUsersService>();
            mockProductsService = mockRepository.Create<IProductsService>();
            mockExamsService = mockRepository.Create<IExamsService>();
            mockVideosService = mockRepository.Create<IVideosService>();
            mockStorageFilesService = mockRepository.Create<IStorageFilesService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.StudentActivitiesService).Returns(mockStudentActivitiesService.Object);
            mockServicesHub.SetupGet(s => s.CertificatesService).Returns(mockCertificatesService.Object);
            mockServicesHub.SetupGet(s => s.CoursesService).Returns(mockCoursesService.Object);
            mockServicesHub.SetupGet(s => s.UsersService).Returns(mockUsersService.Object);
            mockServicesHub.SetupGet(s => s.ProductsService).Returns(mockProductsService.Object);
            mockServicesHub.SetupGet(s => s.ExamsService).Returns(mockExamsService.Object);
            mockServicesHub.SetupGet(s => s.VideosService).Returns(mockVideosService.Object);
            mockServicesHub.SetupGet(s => s.StorageFilesService).Returns(mockStorageFilesService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            studentsApiSettings = new StudentsApiSettings();
            studentsApiSettings.StudentsAdminPermission = "students_admin";
        }

        [Fact]
        public async Task GetStudents_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Student>
            {
                new Student
                {
                    TenantId = user.User.TenantId
                }
            };

            mockStudentsRepository.Setup(r => r.GetAllAsync(user.User.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.User.TenantId);
            mockStudentsRepository.Verify(r => r.GetAllAsync(user.User.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetStudentByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Student>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User.TenantId
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
                        Value = testUser.User.TenantId
                    }
                }
            };

            mockStudentsRepository.Setup(r => r.GetByFilterAsync(filter, testUser.User.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Student> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<StudentDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockStudentsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.User.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetStudentsByFilter_NotFound_Test()
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
                        Value = testUser.User.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<StudentDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockStudentsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.User.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetStudent_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.User.Id,
                TenantId = user.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.User.TenantId);
            mockStudentsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task GetStudentByUser_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.User.Id,
                TenantId = user.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByLoggedUserAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.User.TenantId);
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetStudentByCode_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Code = "123456",
                UserId = user.User.Id,
                TenantId = user.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByStudentCodeAsync(expected.Code)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByCodeAsync(expected.Code, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.User.TenantId);
            mockStudentsRepository.Verify(r => r.GetByStudentCodeAsync(expected.Code), Times.Once);
        }

        [Fact]
        public async Task GetStudentByUserNotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.User.Id,
                TenantId = user.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(() => null);

            // Act
            var result = await service.GetByLoggedUserAsync(user);

            // Assert
            result?.Should().BeNull();
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetStudentByCodeNotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Code = "123456",
                UserId = user.User.Id,
                TenantId = user.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByStudentCodeAsync(expected.Code)).ReturnsAsync(() => null);

            // Act
            var result = await service.GetByCodeAsync(expected.Code, user);

            // Assert
            result?.Should().BeNull();
            mockStudentsRepository.Verify(r => r.GetByStudentCodeAsync(expected.Code), Times.Once);
        }

        [Fact]
        public async Task GetCourse_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedCourse = new CourseDto
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.User.TenantId,
                Disciplines = new List<DisciplineDto>
                {
                    new DisciplineDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test",
                        Lessons = new List<LessonDto>
                        {
                            new LessonDto
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Test"
                            }
                        }
                    }
                }
            };

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                ItemType = "Course",
                ItemId = expectedCourse.Id
            };

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Code = "123456",
                UserId = user.User.Id,
                TenantId = user.User.TenantId,
                Courses = new List<StudentCourse>
                {
                    new StudentCourse
                    {
                        Id = expectedCourse.Id
                    }
                }
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(expected);

            mockProductsService.Setup(s => s.GetByIdAsync(expectedProduct.Id, user)).ReturnsAsync(expectedProduct);

            mockCoursesService.Setup(s => s.GetByIdAsync(expectedCourse.Id)).ReturnsAsync(expectedCourse);

            mockUsersService.Setup(s => s.GetByIdAsync(user.User.Id)).ReturnsAsync(user.User);

            // Act
            var result = await service.GetCourse(user, expected.Courses[0].Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Courses[0].Id);
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetValidateEnrollment_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Code = "123456",
                UserId = user.User.Id,
                TenantId = user.User.TenantId,
                Courses = new List<StudentCourse>
                {
                    new StudentCourse
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                }
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.ValidateEnrollment(user.User.Id, expected.Courses[0].Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Courses[0].Id);
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetCourses_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedCourse = new CourseDto
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.User.TenantId,
                Disciplines = new List<DisciplineDto>
                {
                    new DisciplineDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test",
                        Lessons = new List<LessonDto>
                        {
                            new LessonDto
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Test"
                            }
                        }
                    }
                }
            };

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                ItemType = "Course",
                ItemId = expectedCourse.Id
            };

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Code = "123456",
                UserId = user.User.Id,
                TenantId = user.User.TenantId,
                Courses = new List<StudentCourse>
                {
                    new StudentCourse
                    {
                        Id = expectedCourse.Id
                    }
                }
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(expected);

            mockProductsService.Setup(s => s.GetByIdAsync(expectedProduct.Id, user)).ReturnsAsync(expectedProduct);

            mockCoursesService.Setup(s => s.GetByIdAsync(expectedCourse.Id)).ReturnsAsync(expectedCourse);

            mockUsersService.Setup(s => s.GetByIdAsync(user.User.Id)).ReturnsAsync(user.User);

            // Act
            var result = await service.GetCourses(new RequestFilter(), user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens[0].Id.Should().Be(expected.Courses[0].Id);
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
        }

        [Fact]
        public async Task Enrollment_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedCourse = new CourseDto
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.User.TenantId,
                Disciplines = new List<DisciplineDto>
                {
                    new DisciplineDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test",
                        Lessons = new List<LessonDto>
                        {
                            new LessonDto
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Test"
                            }
                        }
                    }
                }
            };

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                ItemType = "Course",
                ItemId = expectedCourse.Id
            };

            var message = new EnrollmentMessage
            {
                ProductId = expectedProduct.Id,
                OrderId = Guid.NewGuid().ToString(),
                TenantId = user.User.TenantId,
                UserId = user.User.Id
            };

            mockStudentsRepository.Setup(r => r.GetByUserIdAsync(user.User.Id)).ReturnsAsync(() => null);
            mockStudentsRepository.Setup(r => r.SaveAsync(It.IsAny<Student>(), It.IsAny<string>())).ReturnsAsync((Student s, string userId) => s);

            mockProductsService.Setup(s => s.GetByIdAsync(expectedProduct.Id, user)).ReturnsAsync(expectedProduct);

            mockCoursesService.Setup(s => s.GetByIdAsync(expectedCourse.Id)).ReturnsAsync(expectedCourse);

            mockUsersService.Setup(s => s.GetByIdAsync(user.User.Id)).ReturnsAsync(user.User);

            // Act
            var result = await service.Enroll(message, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Courses.Should().HaveCount(1);
            result?.Courses[0].Id.Should().Be(expectedCourse.Id);
            result?.UserId.Should().Be(user.User.Id);
            result?.TenantId.Should().Be(user.User.TenantId);
            result?.Code.Should().NotBeNullOrEmpty();
            mockStudentsRepository.Verify(r => r.GetByUserIdAsync(user.User.Id), Times.Once);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.Is<Student>(s => s.UserId == user.User.Id && s.Courses[0].Id == expectedCourse.Id), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task SaveStudent_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new StudentDto
            {
                Id = string.Empty,
                TenantId = testUser.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.SaveAsync(It.IsAny<Student>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StudentDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.IsAny<Student>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateStudent_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new StudentDto
            {
                Id = string.Empty,
                TenantId = testUser.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StudentDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStudent_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new StudentDto
            {
                Id = expected.Id,
                TenantId = testUser.User.TenantId
            };

            mockUsersService.Setup(s => s.GetByIdAsync(testUser.User.Id)).ReturnsAsync(testUser.User);

            mockStudentsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStudentsRepository.Setup(r => r.SaveAsync(It.IsAny<Student>(), It.IsAny<string>()))
               .ReturnsAsync((Student s, string userId) => s);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StudentDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStudentsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.User?.Permissions.Clear();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.User.TenantId ?? string.Empty
            };

            var input = new StudentDto
            {
                Id = expected.Id,
                TenantId = testUser.User.TenantId
            };

            mockStudentsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStudentsRepository.Setup(r => r.SaveAsync(expected, testUser.User.Id))
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
            mockStudentsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id), Times.Never);
        }

        [Fact]
        public async Task PatchStudent_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Student
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.User.TenantId
            };

            var input = new StudentDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId,
                Code = "Test"
            };

            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockStudentsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockStudentsRepository.Setup(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<StudentDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockStudentsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockStudentsRepository.Verify(r => r.SaveAsync(It.IsAny<Student>(), testUser.User.Id), Times.Once);
        }

        private StudentsService CreateService()
        {
            return new StudentsService(
                mockStudentsRepository.Object,
                mockStudentReportsRepository.Object,
                studentsApiSettings,
                mapper,
                mockEventRegister.Object,
                mockServicesHub.Object);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new LoggedUserDto(new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "students_admin", "staff"],
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            }, "TOKEN");
        }
    }
}