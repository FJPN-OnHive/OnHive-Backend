using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Courses;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Courses.Domain.Mappers;
using OnHive.Courses.Domain.Models;
using OnHive.Courses.Services;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace OnHive.Courses.Tests
{
    public class ExamsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IExamsRepository> mockExamsRepository;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly IMapper mapper;

        public ExamsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockExamsRepository = mockRepository.Create<IExamsRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            coursesApiSettings = new CoursesApiSettings();
            coursesApiSettings.CoursesAdminPermission = "courses_admin";
        }

        [Fact]
        public async Task GetExams_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Exam>
            {
                new Exam
                {
                    TenantId = user.TenantId
                }
            };

            mockExamsRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockExamsRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetExamByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Exam>
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

            mockExamsRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Exam> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<ExamDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockExamsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetExamsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<ExamDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockExamsRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetExam_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockExamsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockExamsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveExam_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new ExamDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockExamsRepository.Setup(r => r.SaveAsync(It.IsAny<Exam>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ExamDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockExamsRepository.Verify(r => r.SaveAsync(It.IsAny<Exam>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateExam_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new ExamDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockExamsRepository.Setup(r => r.SaveAsync(It.IsAny<Exam>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ExamDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockExamsRepository.Verify(r => r.SaveAsync(It.IsAny<Exam>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateExam_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new ExamDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockExamsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockExamsRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ExamDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockExamsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockExamsRepository.Verify(r => r.SaveAsync(It.IsAny<Exam>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task PatchExam_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Exam
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new ExamDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockExamsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockExamsRepository.Setup(r => r.SaveAsync(It.IsAny<Exam>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(JsonSerializer.SerializeToDocument(input), testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ExamDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockExamsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockExamsRepository.Verify(r => r.SaveAsync(It.IsAny<Exam>(), testUser.Id), Times.Once);
        }

        private ExamsService CreateService()
        {
            return new ExamsService(
                mockExamsRepository.Object,
                coursesApiSettings,
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