using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Invoices;
using OnHive.Core.Library.Entities.Tenants;
using OnHive.Invoices.Domain.Abstractions.Repositories;
using OnHive.Invoices.Domain.Mappers;
using OnHive.Invoices.Domain.Models;
using OnHive.Invoices.Services;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Users.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace OnHive.Invoices.Tests
{
    public class InvoicesServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IInvoicesRepository> mockInvoicesRepository;
        private readonly Mock<IUsersService> mockUsersService;
        private readonly Mock<ITenantsService> mockTenantsService;
        private readonly Mock<IOrdersService> mockOrdersService;
        private readonly Mock<ITenantParametersService> mockTenantParametersService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly InvoicesApiSettings invoicesApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public InvoicesServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockInvoicesRepository = mockRepository.Create<IInvoicesRepository>();
            mockUsersService = mockRepository.Create<IUsersService>();
            mockTenantsService = mockRepository.Create<ITenantsService>();
            mockOrdersService = mockRepository.Create<IOrdersService>();
            mockTenantParametersService = mockRepository.Create<ITenantParametersService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(h => h.TenantsService).Returns(mockTenantsService.Object);
            mockServicesHub.SetupGet(h => h.TenantParametersService).Returns(mockTenantParametersService.Object);
            mockServicesHub.SetupGet(h => h.OrdersService).Returns(mockOrdersService.Object);
            mockServicesHub.SetupGet(h => h.UsersService).Returns(mockUsersService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            invoicesApiSettings = new InvoicesApiSettings();
            invoicesApiSettings.InvoicesAdminPermission = "invoices_admin";
            invoicesApiSettings.InvoiceProviders = new List<InvoiceProvider>
            {
                new InvoiceProvider
                {
                    Name = "Default",
                    Url = "http://localhost:1004",
                    IsDefault = true
                }
            };
        }

        [Fact]
        public async Task GetInvoices_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Invoice>
            {
                new Invoice
                {
                    TenantId = loggedUser!.User!.TenantId
                }
            };

            mockInvoicesRepository.Setup(r => r.GetAllAsync(loggedUser!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockInvoicesRepository.Verify(r => r.GetAllAsync(loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetInvoiceByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<Invoice>
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

            mockInvoicesRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId))
                .ReturnsAsync(new PaginatedResult<Invoice> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<InvoiceDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockInvoicesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetInvoicesByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<InvoiceDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockInvoicesRepository.Verify(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetInvoice_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                UserId = loggedUser!.User!.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockInvoicesRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(loggedUser!.User!.TenantId);
            mockInvoicesRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveInvoice_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new InvoiceDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockInvoicesRepository.Setup(r => r.SaveAsync(It.IsAny<Invoice>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<InvoiceDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockInvoicesRepository.Verify(r => r.SaveAsync(It.IsAny<Invoice>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateInvoice_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new InvoiceDto
            {
                Id = string.Empty,
                TenantId = loggedUser!.User!.TenantId
            };

            mockInvoicesRepository.Setup(r => r.SaveAsync(It.IsAny<Invoice>(), loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<InvoiceDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockInvoicesRepository.Verify(r => r.SaveAsync(It.IsAny<Invoice>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateInvoice_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new InvoiceDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockInvoicesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockInvoicesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<InvoiceDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockInvoicesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockInvoicesRepository.Verify(r => r.SaveAsync(It.IsAny<Invoice>(), loggedUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateInvoiceUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();
            loggedUser!.User!.Permissions.Clear();

            var expected = new Invoice
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = loggedUser!.User!.TenantId
            };

            var input = new InvoiceDto
            {
                Id = expected.Id,
                TenantId = loggedUser!.User!.TenantId
            };

            mockInvoicesRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockInvoicesRepository.Setup(r => r.SaveAsync(expected, loggedUser!.User!.Id))
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
            mockInvoicesRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockInvoicesRepository.Verify(r => r.SaveAsync(It.IsAny<Invoice>(), loggedUser!.User!.Id), Times.Never);
        }

        [Fact]
        public async Task InitializeInvoice_ShouldReturnInvoiceDto_WhenCalledWithValidInvoiceMessage()
        {
            // Arrange
            var service = CreateService();

            var client = new UserDto() { Id = Guid.NewGuid().ToString(), Addresses = new List<AddressDto> { new AddressDto() } };
            var emitter = new TenantDto { Id = "11111111111111" };
            var order = new OrderDto { TenantId = "11111111111111", UserId = client.Id, Id = Guid.NewGuid().ToString() };
            var invoiceDto = new InvoiceDto
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = emitter.Id,
                UserId = client.Id,
                Emitter = new InvoiceEmitterDto(),
                Client = new InvoiceClientDto(),
                Itens = new List<InvoiceItensDto>() { new InvoiceItensDto { ProductId = Guid.NewGuid().ToString(), Quantity = 1, UnitValue = 1, TotalValue = 1, Description = "Item" } }
            };
            var invoice = mapper.Map<Invoice>(invoiceDto);
            var invoiceMessage = new InvoiceMessage { OrderId = order.Id, ProviderKey = "", TenantId = emitter.Id };

            mockInvoicesRepository.Setup(s => s.SaveAsync(It.IsAny<Invoice>(), It.IsAny<string>())).ReturnsAsync((Invoice i, string s) => i);

            mockOrdersService.Setup(s => s.GetByIdAsync(order.Id, null)).ReturnsAsync(order);

            mockUsersService.Setup(s => s.GetByIdAsync(client.Id)).ReturnsAsync(client);

            mockTenantsService.Setup(s => s.GetByIdAsync(emitter.Id)).ReturnsAsync(emitter);

            mockTenantParametersService.Setup(s => s.GetByKey(emitter.Id, "INVOICE", "SERIE")).ReturnsAsync(new TenantParameterDto { Value = "1" });

            mockHttpHandler
               .When(HttpMethod.Post, $"{invoicesApiSettings.InvoiceProviders[0].Url}/Internal/Emission")
               .Respond(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new Response<InvoiceDto> { Code = 0, Payload = invoiceDto })));

            // Act
            var result = await service.InitializeInvoice(invoiceMessage);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<InvoiceDto>();
            result.Id.Should().Be(invoiceDto.Id);
            mockInvoicesRepository.Verify(s => s.SaveAsync(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task VerifyPendingInvoices()
        {
            // Arrange
            var service = CreateService();

            var invoiceDto = new InvoiceDto
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                Emitter = new InvoiceEmitterDto(),
                Client = new InvoiceClientDto(),
                Itens = new List<InvoiceItensDto>() { new InvoiceItensDto { ProductId = Guid.NewGuid().ToString(), Quantity = 1, UnitValue = 1, TotalValue = 1, Description = "Item" } },
                Token = Guid.NewGuid().ToString(),
                Status = Core.Library.Enums.Invoices.InvoiceStatus.Authorized
            };
            var invoice = mapper.Map<Invoice>(invoiceDto);

            mockInvoicesRepository.Setup(s => s.SaveAsync(It.IsAny<Invoice>(), It.IsAny<string>())).ReturnsAsync((Invoice i, string s) => i);

            mockInvoicesRepository.Setup(s => s.GetPendingInvoices()).ReturnsAsync(new List<Invoice> { invoice });

            mockHttpHandler
               .When(HttpMethod.Post, $"{invoicesApiSettings.InvoiceProviders[0].Url}/Internal/Verify")
               .Respond(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new Response<InvoiceDto> { Code = 0, Payload = invoiceDto })));

            // Act
            await service.VerifyPendingInvoices();

            // Assert
            mockInvoicesRepository.Verify(s => s.SaveAsync(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Once);
            mockInvoicesRepository.Verify(s => s.GetPendingInvoices(), Times.Once);
        }

        private InvoicesService CreateService()
        {
            return new InvoicesService(
                mockInvoicesRepository.Object,
                invoicesApiSettings,
                mapper,
                httpClient,
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
                    Roles = ["admin", "staff"],
                    Permissions = ["invoices_admin", "admin", "staff"],
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