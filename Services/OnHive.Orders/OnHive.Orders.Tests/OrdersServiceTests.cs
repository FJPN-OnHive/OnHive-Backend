using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Core.Library.Enums.Orders;
using OnHive.Core.Library.Exceptions;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Orders.Domain.Mappers;
using OnHive.Orders.Domain.Models;
using OnHive.Orders.Services;
using OnHive.Payments.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace OnHive.Orders.Tests
{
    public class OrdersServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IOrdersRepository> mockOrdersRepository;
        private readonly Mock<ICartsRepository> mockCartsRepository;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly Mock<IStudentsService> mockStudentsService;
        private readonly Mock<IInvoicesService> mockInvoicesService;
        private readonly Mock<IEmailsService> mockEmailsService;
        private readonly Mock<IPaymentsService> mockPaymentsService;
        private readonly Mock<IProductsService> mockProductsService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly OrdersApiSettings ordersApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public OrdersServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockOrdersRepository = mockRepository.Create<IOrdersRepository>();
            mockCartsRepository = mockRepository.Create<ICartsRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mockStudentsService = mockRepository.Create<IStudentsService>();
            mockInvoicesService = mockRepository.Create<IInvoicesService>();
            mockEmailsService = mockRepository.Create<IEmailsService>();
            mockPaymentsService = mockRepository.Create<IPaymentsService>();
            mockProductsService = mockRepository.Create<IProductsService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.StudentsService).Returns(mockStudentsService.Object);
            mockServicesHub.SetupGet(s => s.InvoicesService).Returns(mockInvoicesService.Object);
            mockServicesHub.SetupGet(s => s.EmailsService).Returns(mockEmailsService.Object);
            mockServicesHub.SetupGet(s => s.PaymentsService).Returns(mockPaymentsService.Object);
            mockServicesHub.SetupGet(s => s.ProductsService).Returns(mockProductsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            ordersApiSettings = new OrdersApiSettings();
            ordersApiSettings.OrdersAdminPermission = "orders_admin";
            ordersApiSettings.InvoiceProvider = "default";
        }

        [Fact]
        public async Task FromCart_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expectedCart =
                new Cart
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Itens = new List<CartItem>
                    {
                        new CartItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            ProductValue = 100.0,
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expectedCart.Id)).ReturnsAsync(expectedCart);
            mockCartsRepository.Setup(r => r.DeleteAsync(expectedCart.Id));
            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id)).ReturnsAsync((Order order, string id) => order);

            // Act
            var result = await service.FromCart(expectedCart.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().ProductId.Should().Be(expectedCart.Itens.First().ProductId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(expectedCart.Id), Times.Once);
            mockCartsRepository.Verify(r => r.DeleteAsync(expectedCart.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id), Times.Once);
        }

        [Theory]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.Pending)]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.PaymentRefused)]
        public async Task Cancel_Test(Core.Library.Enums.Orders.OrderStatus status)
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Status = status,
                    Itens = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            Price = 100.0
                        }
                    }
                };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id)).ReturnsAsync((Order order, string id) => order);

            // Act
            var result = await service.Cancel(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Status.Should().Be(Core.Library.Enums.Orders.OrderStatus.Cancelled);
            result?.Itens.First().ProductId.Should().Be(expected.Itens.First().ProductId);
            mockOrdersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id), Times.Once);
        }

        [Theory]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.Cancelled)]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.Refounded)]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.RefoundRequsted)]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.Closed)]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.PaymentProcessing)]
        public async Task CancelInvalidStatus_Test(Core.Library.Enums.Orders.OrderStatus status)
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Status = status,
                    Itens = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            Price = 100.0
                        }
                    }
                };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id)).ReturnsAsync((Order order, string id) => order);

            // Act
            try
            {
                await service.Cancel(expected.Id, user);
                Assert.Fail("Must raise exceptions.");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidStatusException>();
            }
        }

        [Theory]
        [InlineData(Core.Library.Enums.Orders.OrderStatus.Closed)]
        public async Task Refound_Test(Core.Library.Enums.Orders.OrderStatus status)
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Status = status,
                    Itens = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            Price = 100.0
                        }
                    }
                };

            var recipe = new PaymentReceiptDto
            {
                OrderId = expected.Id,
                PaymentId = Guid.NewGuid().ToString(),
                Status = Core.Library.Enums.Payments.PaymentStatus.Confirmed
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id)).ReturnsAsync((Order order, string id) => order);
            mockPaymentsService.Setup(p => p.GetReceiptByOrder(expected.Id, user)).ReturnsAsync(recipe);
            mockPaymentsService.Setup(p => p.Cancel(recipe.PaymentId, user)).ReturnsAsync(recipe);

            // Act
            var result = await service.RequestRefound(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Status.Should().Be(Core.Library.Enums.Orders.OrderStatus.RefoundRequsted);
            result?.Itens.First().ProductId.Should().Be(expected.Itens.First().ProductId);
            mockOrdersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id), Times.Once);
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Cancelled)]
        [InlineData(OrderStatus.PaymentRefused)]
        [InlineData(OrderStatus.PaymentProcessing)]
        [InlineData(OrderStatus.RefoundRequsted)]
        [InlineData(OrderStatus.Refounded)]
        public async Task RefoundInvalidStatus_Test(OrderStatus status)
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Status = status,
                    Itens = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            Price = 100.0
                        }
                    }
                };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user!.User!.Id)).ReturnsAsync((Order order, string id) => order);

            // Act
            try
            {
                await service.RequestRefound(expected.Id, user);
                Assert.Fail("Must raise exceptions.");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<InvalidStatusException>();
            }
        }

        [Fact]
        public async Task Initialize_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId,
                    Itens = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            Price = 100.0,
                            Sequence = 0
                        }
                    }
                };

            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), user.User.Id)).ReturnsAsync((Order o, string id) => o);
            mockProductsService.Setup(p => p.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.Initialize(expectedProduct.Sku, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().Quantity.Should().Be(1);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.Is<Order>(c => c.Itens.First().ProductId == expectedProduct.Id), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetOrders_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Order>
            {
                new Order
                {
                    TenantId = user!.User!.TenantId
                }
            };

            mockOrdersRepository.Setup(r => r.GetAllAsync(user!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user!.User!.TenantId);
            mockOrdersRepository.Verify(r => r.GetAllAsync(user!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetOrderByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Order>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser !.User !.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser !.User !.TenantId
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
                        Value = testUser !.User !.TenantId
                    }
                }
            };

            mockOrdersRepository.Setup(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId))
                .ReturnsAsync(new PaginatedResult<Order> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<OrderDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockOrdersRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetOrdersByFilter_NotFound_Test()
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
                        Value = testUser !.User !.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<OrderDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockOrdersRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetOrder_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user!.User!.Id,
                TenantId = user!.User!.TenantId
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            mockOrdersRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveOrder_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<OrderDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId,
                UserId = testUser!.User!.Id,
                ExternalId = Guid.NewGuid().ToString(),
                Code = "123456"
            };

            mockOrdersRepository.Setup(r => r.SaveAsync(It.IsAny<Order>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<OrderDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId,
                UserId = testUser!.User!.Id,
                ExternalId = Guid.NewGuid().ToString(),
                Code = "123456"
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockOrdersRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<OrderDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockOrdersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser!.User!.Permissions.Clear();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId ?? string.Empty
            };

            var input = new OrderDto
            {
                Id = expected.Id,
                TenantId = "4444444444444444",
                UserId = testUser!.User!.Id,
                ExternalId = Guid.NewGuid().ToString(),
                Code = "123456"
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockOrdersRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
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
            mockOrdersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.IsAny<Order>(), testUser!.User!.Id), Times.Never);
        }

        [Fact]
        public async Task UpdatePaymentStatus_closed_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockOrdersRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            mockInvoicesService.Setup(e => e.InitializeInvoice(It.IsAny<InvoiceMessage>())).ReturnsAsync(new InvoiceDto());

            // Act
            await service.SetPaymentStatus(input.Id, Guid.NewGuid().ToString(), OrderStatus.Closed, testUser);

            // Assert
            mockOrdersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.Is<Order>(o => o.Status == OrderStatus.Closed), It.IsAny<string>()), Times.Once);
            mockInvoicesService.Verify(e => e.InitializeInvoice(
                It.Is<InvoiceMessage>(m => m.OrderId == expected.Id && m.ProviderKey == ordersApiSettings.InvoiceProvider && m.TenantId == expected.TenantId)), Times.Once);
        }

        [Fact]
        public async Task UpdatePaymentStatus_cancelled_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockOrdersRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            mockInvoicesService.Setup(e => e.InitializeInvoice(It.IsAny<InvoiceMessage>())).ReturnsAsync(new InvoiceDto());

            // Act
            await service.SetPaymentStatus(input.Id, Guid.NewGuid().ToString(), OrderStatus.Cancelled, testUser);

            // Assert
            mockOrdersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.Is<Order>(o => o.Status == OrderStatus.Cancelled), It.IsAny<string>()), Times.Once);
            mockInvoicesService.Verify(e => e.InitializeInvoice(
                It.Is<InvoiceMessage>(m => m.OrderId == expected.Id && m.ProviderKey == ordersApiSettings.InvoiceProvider && m.TenantId == expected.TenantId)), Times.Never);
        }

        [Fact]
        public async Task UpdatePaymentStatus_Refounded_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new OrderDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockOrdersRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockOrdersRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            mockInvoicesService.Setup(e => e.InitializeInvoice(It.IsAny<InvoiceMessage>())).ReturnsAsync(new InvoiceDto());

            // Act
            await service.SetPaymentStatus(input.Id, Guid.NewGuid().ToString(), OrderStatus.Refounded, testUser);

            // Assert
            mockOrdersRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockOrdersRepository.Verify(r => r.SaveAsync(It.Is<Order>(o => o.Status == OrderStatus.Refounded), It.IsAny<string>()), Times.Once);
            mockInvoicesService.Verify(e => e.InitializeInvoice(
               It.Is<InvoiceMessage>(m => m.OrderId == expected.Id && m.ProviderKey == ordersApiSettings.InvoiceProvider && m.TenantId == expected.TenantId)), Times.Never);
        }

        private OrdersService CreateService()
        {
            return new OrdersService(
                mockOrdersRepository.Object,
                ordersApiSettings,
                mockCartsRepository.Object,
                mapper,
                mockEventRegister.Object,
                mockServicesHub.Object);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new LoggedUserDto(new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = ["admin", "staff"],
                Permissions = ["admin", "staff", "orders_admin"],
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            }, "TOKEN");
        }
    }
}