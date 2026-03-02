using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Events;
using EHive.Events.Domain.Abstractions.Repositories;
using EHive.Events.Domain.Mappers;
using EHive.Events.Domain.Models;
using EHive.Events.Services;
using EHive.Users.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace EHive.Events.Tests
{
    public class WebHooksServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IWebHooksRepository> mockEventsRepository;
        private readonly Mock<ILoginService> mockLoginService;
        private readonly EventsApiSettings eventsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public WebHooksServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockEventsRepository = mockRepository.Create<IWebHooksRepository>();
            mockLoginService = mockRepository.Create<ILoginService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            eventsApiSettings = new EventsApiSettings();
            eventsApiSettings.EventsAdminPermission = "events_admin";
        }

        [Fact]
        public async Task GetWebHooks_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<WebHook>
            {
                new WebHook
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockEventsRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockEventsRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetWebHookByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<WebHook>
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

            mockEventsRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true))
                .ReturnsAsync(new PaginatedResult<WebHook> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<WebHookDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockEventsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetWebHooksByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<WebHookDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockEventsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetWebHook_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new WebHook
            {
                Id = Guid.NewGuid().ToString(),
                UserId = loggedUser!.User!.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockEventsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveWebHook_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new WebHook
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new WebHookDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.SaveAsync(It.IsAny<WebHook>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<WebHookDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<WebHook>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateWebHook_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new WebHook
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new WebHookDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.SaveAsync(It.IsAny<WebHook>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<WebHookDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<WebHook>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateWebHook_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new WebHook
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new WebHookDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEventsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<WebHookDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<WebHook>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateWebHookUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new WebHook
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new WebHookDto
            {
                Id = expected.Id,
                TenantId = "222222222222222"
            };

            mockEventsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEventsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockEventsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<WebHook>(), loggedUser!.User!.Id), Times.Never);
        }

        private WebHooksService CreateService()
        {
            return new WebHooksService(
                mockEventsRepository.Object,
                eventsApiSettings,
                mapper,
                mockLoginService.Object);
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
                    Permissions = new List<string> { "admin", "events_admin" },
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