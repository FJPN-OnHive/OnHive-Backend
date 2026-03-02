using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Messages;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Messages;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.Messages.Domain.Abstractions.Repositories;
using EHive.Messages.Domain.Mappers;
using EHive.Messages.Domain.Models;
using EHive.Messages.Services;
using EHive.Users.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using RichardSzalay.MockHttp;

namespace EHive.Messages.Tests
{
    public class MessagesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IMessagesRepository> mockMessagesRepository;
        private readonly Mock<IMessageChannelRepository> mockMessageChannelsRepository;
        private readonly Mock<IMessageUsersRepository> mockMessageUsersRepository;
        private readonly Mock<IEmailsService> mockEmailsService;
        private readonly Mock<IUsersService> mockUsersService;
        private readonly Mock<IUserGroupsService> mockUserGroupsService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly MessagesApiSettings messagesApiSettings;
        private readonly IMapper mapper;

        public MessagesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockMessagesRepository = mockRepository.Create<IMessagesRepository>();
            mockMessageChannelsRepository = mockRepository.Create<IMessageChannelRepository>();
            mockMessageUsersRepository = mockRepository.Create<IMessageUsersRepository>();
            mockEmailsService = mockRepository.Create<IEmailsService>();
            mockUsersService = mockRepository.Create<IUsersService>();
            mockUserGroupsService = mockRepository.Create<IUserGroupsService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.UsersService).Returns(mockUsersService.Object);
            mockServicesHub.SetupGet(s => s.UserGroupsService).Returns(mockUserGroupsService.Object);
            mockServicesHub.SetupGet(s => s.EmailsService).Returns(mockEmailsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            messagesApiSettings = new MessagesApiSettings();
            messagesApiSettings.MessagesAdminPermission = "messages_admin";
        }

        [Fact]
        public async Task GetMessages_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Message>
            {
                new Message
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockMessagesRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockMessagesRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetMessageByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Message>
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

            mockMessagesRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Message> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<MessageDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockMessagesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetMessagesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<MessageDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockMessagesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetMessage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Message
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockMessagesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockMessagesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveMessage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Message
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new MessageDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockMessagesRepository.Setup(r => r.SaveAsync(It.IsAny<Message>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<MessageDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockMessagesRepository.Verify(r => r.SaveAsync(It.IsAny<Message>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateMessage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Message
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new MessageDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockMessagesRepository.Setup(r => r.SaveAsync(It.IsAny<Message>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<MessageDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockMessagesRepository.Verify(r => r.SaveAsync(It.IsAny<Message>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateMessage_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Message
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new MessageDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockMessagesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockMessagesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<MessageDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockMessagesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockMessagesRepository.Verify(r => r.SaveAsync(It.IsAny<Message>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateMessageUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new Message
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new MessageDto
            {
                Id = expected.Id,
                TenantId = "222222222222222"
            };

            mockMessagesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockMessagesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockMessagesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockMessagesRepository.Verify(r => r.SaveAsync(It.IsAny<Message>(), loggedUser!.User!.Id), Times.Never);
        }

        private MessagesService CreateService()
        {
            return new MessagesService(
                mockMessagesRepository.Object,
                mockMessageChannelsRepository.Object,
                mockMessageUsersRepository.Object,
                messagesApiSettings,
                mapper,
                mockServicesHub.Object);
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
                    Permissions = new List<string> { "admin", "messages_admin" },
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