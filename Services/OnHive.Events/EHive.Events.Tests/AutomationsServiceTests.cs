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
    public class AutomationsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IAutomationsRepository> mockAutomationsRepository;
        private readonly IMapper mapper;

        public AutomationsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockAutomationsRepository = mockRepository.Create<IAutomationsRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
        }

        [Fact]
        public async Task GetAutomations_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Automation>
            {
                new Automation
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockAutomationsRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockAutomationsRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetAutomationByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Automation>
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

            mockAutomationsRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Automation> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<AutomationDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockAutomationsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetAutomationsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<AutomationDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockAutomationsRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetAutomation_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockAutomationsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockAutomationsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveAutomation_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new AutomationDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockAutomationsRepository.Setup(r => r.SaveAsync(It.IsAny<Automation>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<AutomationDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockAutomationsRepository.Verify(r => r.SaveAsync(It.IsAny<Automation>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateAutomation_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new AutomationDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockAutomationsRepository.Setup(r => r.SaveAsync(It.IsAny<Automation>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<AutomationDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockAutomationsRepository.Verify(r => r.SaveAsync(It.IsAny<Automation>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateAutomation_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new AutomationDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockAutomationsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockAutomationsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<AutomationDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockAutomationsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockAutomationsRepository.Verify(r => r.SaveAsync(It.IsAny<Automation>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateAutomationUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new Automation
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "2222222"
            };

            var input = new AutomationDto
            {
                Id = expected.Id,
                TenantId = "2222222"
            };

            mockAutomationsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockAutomationsRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockAutomationsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockAutomationsRepository.Verify(r => r.SaveAsync(It.IsAny<Automation>(), loggedUser!.User!.Id), Times.Never);
        }

        private AutomationsService CreateService()
        {
            return new AutomationsService(
                mockAutomationsRepository.Object,
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