using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Events;
using EHive.Core.Library.Contracts.Common;
using EHive.Events.Domain.Abstractions.Repositories;
using EHive.Events.Domain.Mappers;
using EHive.Events.Domain.Models;
using EHive.Events.Services;
using System.Text.Json;

namespace EHive.Events.Tests
{
    public class EventsConfigServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IEventsConfigRepository> mockEventsConfigRepository;
        private readonly IMapper mapper;

        public EventsConfigServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockEventsConfigRepository = mockRepository.Create<IEventsConfigRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
        }

        [Fact]
        public async Task GetEventConfigs_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<EventConfig>
            {
                new EventConfig
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockEventsConfigRepository.Setup(r => r.GetAllAsync(It.IsAny<string>())).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockEventsConfigRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetEventConfigByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<EventConfig>
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

            mockEventsConfigRepository.Setup(r => r.GetByFilterAsync(filter, It.IsAny<string>(), false))
                .ReturnsAsync(new PaginatedResult<EventConfig> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<EventConfigDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockEventsConfigRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetEventConfigsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<EventConfigDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockEventsConfigRepository.Verify(r => r.GetByFilterAsync(filter, It.IsAny<string>(), false), Times.Once);
        }

        [Fact]
        public async Task GetEventConfig_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventConfig
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsConfigRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockEventsConfigRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveEventConfig_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventConfig
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventConfigDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsConfigRepository.Setup(r => r.SaveAsync(It.IsAny<EventConfig>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EventConfigDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.IsAny<EventConfig>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateEventConfig_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventConfig
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventConfigDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsConfigRepository.Setup(r => r.SaveAsync(It.IsAny<EventConfig>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EventConfigDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.IsAny<EventConfig>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEventConfig_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new EventConfig
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new EventConfigDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockEventsConfigRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEventsConfigRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<EventConfigDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockEventsConfigRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.IsAny<EventConfig>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEventConfigUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new EventConfig
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "222222"
            };

            var input = new EventConfigDto
            {
                Id = expected.Id,
                TenantId = "222222"
            };

            mockEventsConfigRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockEventsConfigRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockEventsConfigRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockEventsConfigRepository.Verify(r => r.SaveAsync(It.IsAny<EventConfig>(), loggedUser!.User!.Id), Times.Never);
        }

        private EventsConfigService CreateService()
        {
            return new EventsConfigService(
                mockEventsConfigRepository.Object,
                mapper);
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