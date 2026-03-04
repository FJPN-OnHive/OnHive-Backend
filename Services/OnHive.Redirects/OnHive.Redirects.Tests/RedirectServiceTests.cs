using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Entities.Redirects;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Redirects.Domain.Abstractions.Repositories;
using OnHive.Redirects.Domain.Mappers;
using OnHive.Redirects.Domain.Models;
using OnHive.Redirects.Services;
using System.Text.Json;

namespace OnHive.Redirects.Tests
{
    public class RedirectServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IRedirectRepository> mockRedirectRepository;
        private readonly RedirectApiSettings redirectApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public RedirectServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockRedirectRepository = mockRepository.Create<IRedirectRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            redirectApiSettings = new RedirectApiSettings();
            redirectApiSettings.RedirectAdminPermission = "redirect_admin";
        }

        [Fact]
        public async Task GetRedirects_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Redirect>
            {
                new Redirect
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockRedirectRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockRedirectRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetRedirectByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Redirect>
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

            mockRedirectRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Redirect> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<RedirectDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockRedirectRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetRedirectsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<RedirectDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockRedirectRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetRedirect_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Redirect
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            mockRedirectRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockRedirectRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveRedirect_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Redirect
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new RedirectDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockRedirectRepository.Setup(r => r.SaveAsync(It.IsAny<Redirect>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RedirectDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRedirectRepository.Verify(r => r.SaveAsync(It.IsAny<Redirect>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateRedirect_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Redirect
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new RedirectDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockRedirectRepository.Setup(r => r.SaveAsync(It.IsAny<Redirect>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RedirectDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRedirectRepository.Verify(r => r.SaveAsync(It.IsAny<Redirect>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateRedirect_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Redirect
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new RedirectDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockRedirectRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockRedirectRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<RedirectDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockRedirectRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockRedirectRepository.Verify(r => r.SaveAsync(It.IsAny<Redirect>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateRedirectUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions?.Clear();

            var expected = new Redirect
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "22222222"
            };

            var input = new RedirectDto
            {
                Id = expected.Id,
                TenantId = "22222222"
            };

            mockRedirectRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockRedirectRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockRedirectRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockRedirectRepository.Verify(r => r.SaveAsync(It.IsAny<Redirect>(), loggedUser!.User!.Id), Times.Never);
        }

        private RedirectService CreateService()
        {
            return new RedirectService(
                mockRedirectRepository.Object,
                redirectApiSettings,
                mapper,
                httpClient);
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
                    Permissions = new List<string> { "admin", "redirect_admin" },
                    Roles = new List<string> { "admin", "staff" },
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