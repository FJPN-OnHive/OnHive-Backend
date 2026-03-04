using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Mappers;
using OnHive.Orders.Domain.Models;
using OnHive.Orders.Services;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;
using System.Text.Json;

namespace OnHive.Orders.Tests
{
    public class CartsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ICartsRepository> mockCartsRepository;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly Mock<IOrdersService> mockOrdersService;
        private readonly Mock<IProductsService> mockProductsService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly OrdersApiSettings ordersApiSettings;
        private readonly IMapper mapper;

        public CartsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockCartsRepository = mockRepository.Create<ICartsRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mockOrdersService = mockRepository.Create<IOrdersService>();
            mockProductsService = mockRepository.Create<IProductsService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.ProductsService).Returns(mockProductsService.Object);
            mockServicesHub.SetupGet(s => s.OrdersService).Returns(mockOrdersService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            ordersApiSettings = new OrdersApiSettings();
            ordersApiSettings.OrdersAdminPermission = "orders_admin";
        }

        [Fact]
        public async Task GetCarts_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Cart>
            {
                new Cart
                {
                    TenantId = user!.User!.TenantId
                }
            };

            mockCartsRepository.Setup(r => r.GetAllAsync(user.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user!.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetAllAsync(user!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCartsByUser_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Cart>
            {
                new Cart
                {
                    TenantId = user!.User!.TenantId
                }
            };

            mockCartsRepository.Setup(r => r.GetByUserIdAsync(user.User!.Id, user.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByUser(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user!.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetByUserIdAsync(user.User!.Id, user.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task AddProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected =
                new Cart
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = user!.User!.TenantId
                };

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), user.User.Id)).ReturnsAsync(expected);
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.AddProduct(expected.Id, expectedProduct.Sku, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().ProductValue.Should().Be(expectedProduct.FullPrice);
            result?.Itens.First().Quantity.Should().Be(1);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.Any(i => i.ProductId == expectedProduct.Id)), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task AddProductExisting_Test()
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

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), user.User.Id)).ReturnsAsync(expected);
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.AddProduct(expected.Id, expectedProduct.Sku, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().ProductValue.Should().Be(expectedProduct.FullPrice);
            result?.Itens.First().Quantity.Should().Be(2);

            result?.TenantId.Should().Be(user!.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.Any(i => i.ProductId == expectedProduct.Id)), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task RemoveProduct_Test()
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
                            Sequence = 0
                        },
                         new CartItem
                        {
                            ProductId= Guid.NewGuid().ToString(),
                            Quantity = 1,
                            ProductValue = 200.0,
                            Sequence = 1
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), user.User.Id)).ReturnsAsync(expected);
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.RemoveProduct(expected.Id, 0, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().ProductValue.Should().Be(200);
            result?.Itens.First().Sequence.Should().Be(0);
            result?.Itens.First().Quantity.Should().Be(1);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.Count == 1), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task RemoveLastProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var id = Guid.NewGuid().ToString();

            var expectedProduct = new ProductDto
            {
                Id = Guid.NewGuid().ToString(),
                Sku = "123456789",
                FullPrice = 100.0F
            };

            var expected =
                new Cart
                {
                    Id = id,
                    TenantId = user!.User!.TenantId,
                    Itens = new List<CartItem>
                    {
                        new CartItem
                        {
                            ProductId= expectedProduct.Id,
                            Quantity = 1,
                            ProductValue = 100.0,
                            Sequence = 0
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.DeleteAsync(expected.Id));
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.RemoveProduct(expected.Id, 0, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            result?.Id.Should().BeNullOrEmpty();
            mockCartsRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task ChangeProduct_Test()
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
                            Sequence = 0
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.DeleteAsync(expected.Id));
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.ChangeProductQuantity(expected.Id, 0, 3, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().Quantity.Should().Be(3);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.First().Quantity == 3 && c.Itens.First().ProductId == expectedProduct.Id), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task ChangeProductToZero_Test()
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
                            Sequence = 0
                        },
                        new CartItem
                        {
                            ProductId= Guid.NewGuid().ToString(),
                            Quantity = 1,
                            ProductValue = 100.0,
                            Sequence = 1
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockCartsRepository.Setup(r => r.DeleteAsync(expected.Id));
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.ChangeProductQuantity(expected.Id, 1, 0, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().Quantity.Should().Be(1);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.First().Quantity == 1 && c.Itens.First().ProductId == expectedProduct.Id), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteCart_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var id = Guid.NewGuid().ToString();

            var expected =
                new Cart
                {
                    Id = id,
                    TenantId = user!.User!.TenantId
                };

            mockCartsRepository.Setup(r => r.DeleteAsync(expected.Id));

            // Act
            await service.Delete(expected.Id, user);

            // Assert
            mockCartsRepository.Verify(r => r.DeleteAsync(id), Times.Once);
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
                            Sequence = 0
                        }
                    }
                };

            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), user.User.Id)).ReturnsAsync(expected); ;
            mockProductsService.Setup(r => r.GetBySkuAsync(expectedProduct.Sku, user)).ReturnsAsync(expectedProduct);

            // Act
            var result = await service.Initialize(expectedProduct.Sku, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Itens.Should().HaveCount(1);
            result?.Itens.First().Quantity.Should().Be(1);
            mockCartsRepository.Verify(r => r.SaveAsync(It.Is<Cart>(c => c.Itens.First().ProductId == expectedProduct.Id), user.User.Id), Times.Once);
        }

        [Fact]
        public async Task GetCartByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Cart>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser !.User !.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser !.User !.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser.User!.TenantId
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

            mockCartsRepository.Setup(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId))
                .ReturnsAsync(new PaginatedResult<Cart> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CartDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockCartsRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCartsByFilter_NotFound_Test()
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
            result?.Should().BeOfType<PaginatedResult<CartDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockCartsRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCart_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.User!.Id,
                TenantId = user.User!.TenantId
            };

            mockCartsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user.User!.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveCart_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new CartDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CartDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCartsRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateCart_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new CartDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CartDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCartsRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCart_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new CartDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockCartsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCartsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CartDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCartsRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCartUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser!.User!.Permissions.Clear();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser?.User!.TenantId ?? string.Empty
            };

            var input = new CartDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockCartsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCartsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
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
            mockCartsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockCartsRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id), Times.Never);
        }

        [Fact]
        public async Task PatchCart_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new CartDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId,
                Code = "Test"
            };
            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockCartsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCartsRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CartDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCartsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCartsRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), testUser!.User!.Id), Times.Once);
        }

        private CartsService CreateService()
        {
            return new CartsService(
                mockCartsRepository.Object,
                ordersApiSettings,
                mapper,
                mockEventRegister.Object,
                mockServicesHub.Object);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = "11111111111111";
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