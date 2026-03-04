using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Entities.Events;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Events.Domain.Abstractions.Repositories;
using OnHive.Events.Domain.Mappers;
using OnHive.Events.Domain.Models;
using OnHive.Events.Services;
using OnHive.Core.Library.Enums.Events;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit;

namespace OnHive.Events.Tests
{
    public class EventsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IEventsRepository> mockEventsRepository;
        private readonly Mock<IEventsConfigRepository> mockEventsConfigRepository;
        private readonly Mock<IAutomationsRepository> mockAutomationRepository;
        private readonly EventsApiSettings eventsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;
        private Mock<ISmtpClient> mockSmtpClient;

        public EventsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockEventsRepository = mockRepository.Create<IEventsRepository>();
            mockEventsConfigRepository = mockRepository.Create<IEventsConfigRepository>();
            mockAutomationRepository = mockRepository.Create<IAutomationsRepository>();
            mockSmtpClient = mockRepository.Create<ISmtpClient>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            eventsApiSettings = new EventsApiSettings();
            eventsApiSettings.EventsAdminPermission = "events_admin";
        }

        [Theory]
        [InlineData(AutomationWebHookMethod.POST)]
        [InlineData(AutomationWebHookMethod.PATCH)]
        [InlineData(AutomationWebHookMethod.PUT)]
        [InlineData(AutomationWebHookMethod.GET)]
        public async Task ProcessEventMessage_WithWebHook_Test(AutomationWebHookMethod method)
        {
            // Arrange
            var service = CreateService();
            var eventMessage = new EventMessage
            {
                Key = "new_event",
                TenantId = "11111111111111",
                Date = DateTime.Now,
                Fields = new Dictionary<string, string>
                {
                    { "field1", "value1" },
                    { "field2", "value2" }
                },
                Message = "New Event",
                Origin = "Test",
                UserId = Guid.NewGuid().ToString(),
                Tags = ["tag1", "tag2"]
            };
            var automation = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = eventMessage.TenantId,
                EventKey = eventMessage.Key,
                Type = AutomationType.WebHook,
                Conditions = new List<AutomationCondition>
                {
                    new AutomationCondition
                    {
                        Field = "field1",
                        Type = AutomationConditionType.equal,
                        Condition = "value1"
                    }
                },
                WebHook = new AutomationWebHook
                {
                    Url = "http://test.com/{{field2}}",
                    Method = method,
                    Body = "Test {{field1}}",
                    ContentType = "application/json",
                    Headers = new Dictionary<string, string>
                    {
                        { "header1", "value1" },
                        { "header2", "value2" }
                    },
                }
            };

            mockEventsConfigRepository.Setup(r => r.GetByKeyAndOrigin(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => default);
            mockAutomationRepository.Setup(r => r.GetByKey(eventMessage.TenantId, eventMessage.Key)).ReturnsAsync(() => [automation]);
            if (method != AutomationWebHookMethod.GET)
            {
                mockHttpHandler.When("http://test.com/value2").Respond("application/json", "OK")
                    .WithContent("Test value1")
                    .WithHeaders(automation.WebHook.Headers);
            }
            else
            {
                mockHttpHandler.When("http://test.com/value2").Respond("application/json", "OK")
                    .WithHeaders(automation.WebHook.Headers);
            }

            // Act
            await service.ProcessEvent(eventMessage, true);

            // Assert
            mockEventsRepository.Verify(r => r.SaveAsync(It.Is<EventRegister>(e =>
                 e.Key == eventMessage.Key
                 && e.Origin == eventMessage.Origin
                 && e.Fields.All(f => eventMessage.Fields.Keys.Contains(f.Key))), It.IsAny<string>()), Times.Once);

            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.Is<EventConfig>(e =>
                e.Key == eventMessage.Key
                && e.Origin == eventMessage.Origin
                && e.Fields.TrueForAll(f => eventMessage.Fields.Keys.Contains(f.Key))), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessEventMessage_WithEmail_Test()
        {
            // Arrange
            var service = CreateService();
            var eventMessage = new EventMessage
            {
                Key = "new_event",
                TenantId = "11111111111111",
                Date = DateTime.Now,
                Fields = new Dictionary<string, string>
                {
                    { "field1", "value1" },
                    { "field2", "value2" }
                },
                Message = "New Event",
                Origin = "Test",
                UserId = Guid.NewGuid().ToString(),
                Tags = ["tag1", "tag2"]
            };
            var automation = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = eventMessage.TenantId,
                EventKey = eventMessage.Key,
                Type = AutomationType.Email,
                Conditions = new List<AutomationCondition>
                {
                    new AutomationCondition
                    {
                        Field = "field1",
                        Type = AutomationConditionType.equal,
                        Condition = "value1"
                    }
                },
                Email = new AutomationEmail
                {
                    To = "Test",
                    Body = "Test {{field1}}",
                    Subject = "Test {{field2}}"
                }
            };

            mockEventsConfigRepository.Setup(r => r.GetByKeyAndOrigin(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => default);
            mockAutomationRepository.Setup(r => r.GetByKey(eventMessage.TenantId, eventMessage.Key)).ReturnsAsync(() => [automation]);

            // Act
            await service.ProcessEvent(eventMessage, true);

            // Assert
            mockEventsRepository.Verify(r => r.SaveAsync(It.Is<EventRegister>(e =>
                 e.Key == eventMessage.Key
                 && e.Origin == eventMessage.Origin
                 && e.Fields.All(f => eventMessage.Fields.Keys.Contains(f.Key))), It.IsAny<string>()), Times.Once);

            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.Is<EventConfig>(e =>
                e.Key == eventMessage.Key
                && e.Origin == eventMessage.Origin
                && e.Fields.TrueForAll(f => eventMessage.Fields.Keys.Contains(f.Key))), It.IsAny<string>()), Times.Once);

            mockSmtpClient.Verify(r => r.ConnectAsync(eventsApiSettings.EmailService.Server, eventsApiSettings.EmailService.Port, It.IsAny<MailKit.Security.SecureSocketOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockSmtpClient.Verify(r => r.AuthenticateAsync(eventsApiSettings.EmailService.User, eventsApiSettings.EmailService.User, It.IsAny<CancellationToken>()), Times.Once);
            mockSmtpClient.Verify(r => r.SendAsync(It.Is<MimeMessage>(e => e.Subject.Equals("Test value2")), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
            mockSmtpClient.Verify(r => r.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetEventRegisters_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<EventRegister>
            {
                new EventRegister
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
        public async Task GetEventRegisterByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<EventRegister>
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

            mockEventsRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<EventRegister> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<EventRegisterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockEventsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetEventRegistersByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<EventRegisterDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockEventsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetEventRegister_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventRegister
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
        public async Task SaveEventRegister_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventRegister
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventRegisterDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.SaveAsync(It.IsAny<EventRegister>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EventRegisterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<EventRegister>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateEventRegister_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventRegister
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventRegisterDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsRepository.Setup(r => r.SaveAsync(It.IsAny<EventRegister>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EventRegisterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<EventRegister>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEventRegister_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventRegister
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventRegisterDto
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
            result?.Should().BeOfType<EventRegisterDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<EventRegister>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEventRegisterUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new EventRegister
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "22222222222222"
            };

            var input = new EventRegisterDto
            {
                Id = expected.Id,
                TenantId = "22222222222222"
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
            mockEventsRepository.Verify(r => r.SaveAsync(It.IsAny<EventRegister>(), loggedUser!.User!.Id), Times.Never);
        }

        private EventsService CreateService()
        {
            return new EventsService(
                mockEventsRepository.Object,
                eventsApiSettings,
                mockAutomationRepository.Object,
                mockEventsConfigRepository.Object,
                mapper,
                httpClient,
                mockSmtpClient.Object);
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
                    Roles = ["admin", "staff"],
                    Permissions = ["admin", "events_admin", "staff"],
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