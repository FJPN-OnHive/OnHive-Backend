using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using EHive.Core.Library.Contracts.Teachers;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Teachers;
using EHive.Core.Library.Contracts.Common;
using EHive.Teachers.Domain.Abstractions.Repositories;
using EHive.Teachers.Domain.Mappers;
using EHive.Teachers.Domain.Models;
using EHive.Teachers.Services;
using System.Text.Json;

namespace EHive.Teachers.Tests
{
    public class TeachersServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ITeachersRepository> mockTeachersRepository;
        private readonly TeachersApiSettings teachersApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public TeachersServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockTeachersRepository = mockRepository.Create<ITeachersRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            teachersApiSettings = new TeachersApiSettings();
            teachersApiSettings.TeachersAdminPermission = "teachers_admin";
        }

        [Fact]
        public async Task GetTeachers_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Teacher>
            {
                new Teacher
                {
                    TenantId = user.TenantId
                }
            };

            mockTeachersRepository.Setup(r => r.GetAllAsync(user.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user.TenantId);
            mockTeachersRepository.Verify(r => r.GetAllAsync(user.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTeacherByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Teacher>
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

            mockTeachersRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId))
                .ReturnsAsync(new PaginatedResult<Teacher> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<TeacherDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockTeachersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTeachersByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<TeacherDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockTeachersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetTeacher_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Teacher
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                TenantId = user.TenantId
            };

            mockTeachersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.TenantId);
            mockTeachersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveTeacher_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Teacher
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TeacherDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTeachersRepository.Setup(r => r.SaveAsync(It.IsAny<Teacher>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TeacherDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTeachersRepository.Verify(r => r.SaveAsync(It.IsAny<Teacher>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateTeacher_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Teacher
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TeacherDto
            {
                Id = string.Empty,
                TenantId = testUser.TenantId
            };

            mockTeachersRepository.Setup(r => r.SaveAsync(It.IsAny<Teacher>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TeacherDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTeachersRepository.Verify(r => r.SaveAsync(It.IsAny<Teacher>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTeacher_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Teacher
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new TeacherDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTeachersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTeachersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<TeacherDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockTeachersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockTeachersRepository.Verify(r => r.SaveAsync(It.IsAny<Teacher>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateTeacherUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new Teacher
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.TenantId ?? string.Empty
            };

            var input = new TeacherDto
            {
                Id = expected.Id,
                TenantId = testUser.TenantId
            };

            mockTeachersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockTeachersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockTeachersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockTeachersRepository.Verify(r => r.SaveAsync(It.IsAny<Teacher>(), testUser.Id), Times.Never);
        }

        private TeachersService CreateService()
        {
            return new TeachersService(
                mockTeachersRepository.Object,
                teachersApiSettings,
                mapper,
                httpClient);
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
                Permissions = ["admin", "teachers_admin", "staff"],
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