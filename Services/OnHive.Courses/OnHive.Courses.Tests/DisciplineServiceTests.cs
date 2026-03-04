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
    public class DisciplineServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IDisciplineRepository> mockDisciplineRepository;
        private readonly CoursesApiSettings coursesApiSettings;
        private readonly Mock<IExamsService> mockExamsService;
        private readonly Mock<ILessonsService> mockLessonsService;
        private readonly IMapper mapper;

        public DisciplineServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockDisciplineRepository = mockRepository.Create<IDisciplineRepository>();
            mockLessonsService = mockRepository.Create<ILessonsService>();
            mockExamsService = mockRepository.Create<IExamsService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            coursesApiSettings = new CoursesApiSettings();
            coursesApiSettings.CoursesAdminPermission = "courses_admin";
        }

        [Fact]
        public async Task GetDisciplines_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Discipline>
            {
                new Discipline
                {
                    TenantId = user.TenantId
                }
            };

            mockDisciplineRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockDisciplineRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetDisciplineByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Discipline>
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

            mockDisciplineRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Discipline> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<DisciplineDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockDisciplineRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetDisciplinesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<DisciplineDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockDisciplineRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetDiscipline_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Discipline
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockDisciplineRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, new UserDto());

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockDisciplineRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveDiscipline_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Discipline
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new DisciplineDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockDisciplineRepository.Setup(r => r.SaveAsync(It.IsAny<Discipline>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<DisciplineDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDisciplineRepository.Verify(r => r.SaveAsync(It.IsAny<Discipline>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateDiscipline_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Discipline
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new DisciplineDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockDisciplineRepository.Setup(r => r.SaveAsync(It.IsAny<Discipline>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<DisciplineDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDisciplineRepository.Verify(r => r.SaveAsync(It.IsAny<Discipline>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateDiscipline_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Discipline
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new DisciplineDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockDisciplineRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockDisciplineRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<DisciplineDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDisciplineRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockDisciplineRepository.Verify(r => r.SaveAsync(It.IsAny<Discipline>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task PatchDiscipline_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Discipline
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new DisciplineDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockDisciplineRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockDisciplineRepository.Setup(r => r.SaveAsync(It.IsAny<Discipline>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(JsonSerializer.SerializeToDocument(input), testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<DisciplineDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockDisciplineRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockDisciplineRepository.Verify(r => r.SaveAsync(It.IsAny<Discipline>(), testUser.Id), Times.Once);
        }

        private DisciplineService CreateService()
        {
            return new DisciplineService(
                mockDisciplineRepository.Object,
                coursesApiSettings,
                mapper,
                mockLessonsService.Object,
                mockExamsService.Object);
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