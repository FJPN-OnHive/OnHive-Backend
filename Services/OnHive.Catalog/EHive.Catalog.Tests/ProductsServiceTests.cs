using AutoMapper;
using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Catalog.Domain.Mappers;
using EHive.Catalog.Domain.Models;
using EHive.Catalog.Services;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Catalog;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace EHive.Catalog.Tests
{
    public class ProductsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IProductsRepository> mockProductsRepository;
        private readonly CatalogApiSettings productsApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public ProductsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockProductsRepository = mockRepository.Create<IProductsRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            productsApiSettings = new CatalogApiSettings();
            productsApiSettings.CatalogAdminPermission = "products_admin";
            productsApiSettings.ProductTypes = new List<ProductType>
            {
                new ProductType { BaseUrl = "http://localhost/v1/course/", IsDefault = true, Key = "course"}
            };
        }

        [Fact]
        public async Task GetProducts_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Product>
            {
                new Product
                {
                    TenantId = user!.User!.TenantId
                }
            };

            mockProductsRepository.Setup(r => r.GetAllAsync(user!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user!.User!.TenantId);
            mockProductsRepository.Verify(r => r.GetAllAsync(user!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetProductByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Product>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId,
                   IsActive  = true
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId,
                   IsActive  = true
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId,
                   IsActive  = true
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
                        Value = testUser!.User!.TenantId
                    }
                }
            };

            mockProductsRepository.Setup(r => r.GetByFilterActiveAsync(filter, testUser!.User!.TenantId))
                .ReturnsAsync(new PaginatedResult<Product> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser!.User!.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<ProductDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockProductsRepository.Verify(r => r.GetByFilterActiveAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetProductByFilterLogged_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Product>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = testUser!.User!.TenantId
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
                        Value = testUser!.User!.TenantId
                    }
                }
            };

            mockProductsRepository.Setup(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId, false))
                .ReturnsAsync(new PaginatedResult<Product> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<ProductDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockProductsRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId, false), Times.Once);
        }

        [Fact]
        public async Task GetProductsByFilter_NotFound_Test()
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
                        Value = testUser!.User!.TenantId
                    }
                }
            };

            // Act
            var result = await service.GetByFilterAsync(filter, testUser!.User!.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<ProductDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockProductsRepository.Verify(r => r.GetByFilterActiveAsync(filter, testUser!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user!.User!.TenantId,
                ItemId = "123456",
                ItemType = "course"
            };

            mockProductsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);
            mockHttpHandler.When($"http://localhost/v1/course/{expected.ItemId}").Respond(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new Response<CourseDto>())));

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            result?.Item.Should().NotBeNull();
            mockProductsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task GetProductBySku_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user!.User!.TenantId,
                Sku = "ABC",
                ItemId = "123456",
                ItemType = "course"
            };

            mockProductsRepository.Setup(r => r.GetBySku(user!.User!.TenantId, expected.Sku)).ReturnsAsync(expected);
            mockHttpHandler.When($"http://localhost/v1/course/{expected.ItemId}").Respond(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new Response<CourseDto>())));

            // Act
            var result = await service.GetBySkuAsync(expected.Sku, user!.User!.TenantId);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            result?.Item.Should().NotBeNull();
            mockProductsRepository.Verify(r => r.GetBySku(user!.User!.TenantId, expected.Sku), Times.Once);
        }

        [Fact]
        public async Task GetProductBySkuLogged_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user!.User!.TenantId,
                Sku = "ABC",
                ItemId = "123456",
                ItemType = "course"
            };

            mockProductsRepository.Setup(r => r.GetBySku(user!.User!.TenantId, expected.Sku)).ReturnsAsync(expected);
            mockHttpHandler.When($"http://localhost/v1/course/{expected.ItemId}").Respond(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(new Response<CourseDto>())));

            // Act
            var result = await service.GetBySkuAsync(expected.Sku, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            result?.Item.Should().NotBeNull();
            mockProductsRepository.Verify(r => r.GetBySku(user!.User!.TenantId, expected.Sku), Times.Once);
        }

        [Fact]
        public async Task SaveProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new ProductDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockProductsRepository.Setup(r => r.SaveAsync(It.IsAny<Product>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ProductDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockProductsRepository.Verify(r => r.SaveAsync(It.IsAny<Product>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new ProductDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockProductsRepository.Setup(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ProductDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockProductsRepository.Verify(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new ProductDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId
            };

            mockProductsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockProductsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ProductDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockProductsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockProductsRepository.Verify(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateProductUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser!.User!.Permissions.Clear();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId ?? string.Empty
            };

            var input = new ProductDto
            {
                Id = expected.Id,
                TenantId = "22222222222222"
            };

            mockProductsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockProductsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
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
            mockProductsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockProductsRepository.Verify(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id), Times.Never);
        }

        [Fact]
        public async Task PatchProduct_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Product
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new ProductDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId,
                Name = "Test"
            };
            var inputJson = JsonDocument.Parse(JsonSerializer.Serialize(input));

            mockProductsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockProductsRepository.Setup(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.PatchAsync(inputJson, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<ProductDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockProductsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockProductsRepository.Verify(r => r.SaveAsync(It.IsAny<Product>(), testUser!.User!.Id), Times.Once);
        }

        private ProductsService CreateService()
        {
            return new ProductsService(
                mockProductsRepository.Object,
                productsApiSettings,
                mapper,
                httpClient);
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
                Permissions = ["admin", "products_admin", "staff"],
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