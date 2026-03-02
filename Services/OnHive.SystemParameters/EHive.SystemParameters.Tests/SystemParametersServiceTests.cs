using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.SystemParameters;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.SystemParameters;
using EHive.SystemParameters.Domain.Abstractions.Repositories;
using EHive.SystemParameters.Domain.Mappers;
using EHive.SystemParameters.Domain.Models;
using EHive.SystemParameters.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;

namespace EHive.SystemParameters.Tests
{
    public class SystemParametersServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ISystemParametersRepository> mockSystemParametersRepository;
        private readonly SystemParametersApiSettings systemParametersApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public SystemParametersServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockSystemParametersRepository = mockRepository.Create<ISystemParametersRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            systemParametersApiSettings = new SystemParametersApiSettings();
            systemParametersApiSettings.SystemParametersAdminPermission = "systemParameters_admin";
        }

        [Fact]
        public async Task GetSystemParameters_Test()
        {
            // Arrange
            var service = CreateService();

            var expected = new List<SystemParameter>
            {
                new SystemParameter
                {
                    Id = "TEST_PARAMETER"
                }
            };

            mockSystemParametersRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            mockSystemParametersRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetSystemParameterByGroup()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var group = "GROUP";

            var expected = new List<SystemParameter>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   Group = group
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   Group = group
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   Group = group
                }
            };

            mockSystemParametersRepository.Setup(r => r.GetByGroupAsync(group)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByGroupAsync(group);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<List<SystemParameterDto>>();
            result?.Should().NotBeNull();
            result?.Should().HaveCount(3);
            result?.All(p => expected.Select(e => e.Id).Contains(p.Id)).Should().BeTrue();
            mockSystemParametersRepository.Verify(r => r.GetByGroupAsync(group), Times.Once);
        }

        [Fact]
        public async Task GetSystemParameterByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<SystemParameter>
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

            mockSystemParametersRepository.Setup(r => r.GetByFilterAsync(filter, testUser.TenantId, true))
                .ReturnsAsync(new PaginatedResult<SystemParameter> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<SystemParameterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockSystemParametersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetSystemParametersByFilter_NotFound_Test()
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
                    new FilterField                    {
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
            result?.Should().BeOfType<PaginatedResult<SystemParameterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockSystemParametersRepository.Verify(r => r.GetByFilterAsync(filter, testUser.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetSystemParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new SystemParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user.TenantId
            };

            mockSystemParametersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            mockSystemParametersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveSystemParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new SystemParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new SystemParameterDto
            {
                Id = string.Empty
            };

            mockSystemParametersRepository.Setup(r => r.SaveAsync(It.IsAny<SystemParameter>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<SystemParameterDto>();
            mockSystemParametersRepository.Verify(r => r.SaveAsync(It.IsAny<SystemParameter>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateSystemParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new SystemParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new SystemParameterDto
            {
                Id = string.Empty
            };

            mockSystemParametersRepository.Setup(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<SystemParameterDto>();
            mockSystemParametersRepository.Verify(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateSystemParameter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new SystemParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser.TenantId
            };

            var input = new SystemParameterDto
            {
                Id = expected.Id
            };

            mockSystemParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockSystemParametersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<SystemParameterDto>();
            mockSystemParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockSystemParametersRepository.Verify(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateSystemParameterUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser?.Permissions.Clear();

            var expected = new SystemParameter
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.TenantId ?? string.Empty
            };

            var input = new SystemParameterDto
            {
                Id = expected.Id
            };

            mockSystemParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockSystemParametersRepository.Setup(r => r.SaveAsync(expected, testUser.Id))
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
            mockSystemParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockSystemParametersRepository.Verify(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id), Times.Never);
        }

        //[Fact]
        //public async Task PatchSystemParameter_Test()
        //{
        //    // Arrange
        //    var service = CreateService();

        //    var testUser = GetTestUser();

        //    var expected = new SystemParameter
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        TenantId = testUser.TenantId
        //    };

        //    var input = new MockedRequestJsonExtensions
        //    {
        //        Id = expected.Id,
        //        TenantId = testUser.TenantId,
        //        Fields = new Dictionary<string, object>
        //        {
        //            { "fieldName1", "value" }
        //        }
        //    };

        //    mockSystemParametersRepository.Setup(r => r.GetByIdAsync(input.Id))
        //       .ReturnsAsync(expected);

        //    mockSystemParametersRepository.Setup(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id))
        //       .ReturnsAsync(expected);

        //    // Act
        //    var result = await service.UpdateAsync(input, testUser);

        //    // Assert
        //    result?.Should().NotBeNull();
        //    result?.Should().BeOfType<SystemParameterDto>();
        //    mockSystemParametersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
        //    mockSystemParametersRepository.Verify(r => r.SaveAsync(It.IsAny<SystemParameter>(), testUser.Id), Times.Once);
        //}

        [Fact]
        public async Task Migrate()
        {
            // Arrange
            var service = CreateService();

            var expected = new List<SystemParameter>
            {
                new SystemParameter
                {
                    Id = "LMS_DOMAIN",
                    Group = "SYSTEM_DOMAINS",
                    Value = "TestValue"
                }
            };

            mockSystemParametersRepository.Setup(r => r.GetAllAsync())
               .ReturnsAsync(expected);

            mockSystemParametersRepository.Setup(r => r.SaveAsync(It.IsAny<SystemParameter>(), It.IsAny<string>()))
               .ReturnsAsync((SystemParameter input, string userId) => input);

            // Act
            await service.Migrate();

            // Assert
            mockSystemParametersRepository.Verify(r => r.GetAllAsync(), Times.Once);
            mockSystemParametersRepository.Verify(r => r.SaveAsync(It.Is<SystemParameter>(p => p.Id != expected[0].Id), It.IsAny<string>()), Times.Exactly(2));
        }

        private SystemParametersService CreateService()
        {
            return new SystemParametersService(
                mockSystemParametersRepository.Object,
                systemParametersApiSettings,
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
                Permissions = ["admin", "staff", "systemParameters_admin"],
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