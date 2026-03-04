using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Repositories;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Catalog.Domain.Mappers;
using OnHive.Catalog.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Core.Library.Enums.Catalog;
using FluentAssertions;
using Moq;
using OnHive.Domains.Common.Abstractions.Services;

namespace OnHive.Catalog.Tests
{
    public class CouponsServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ICouponsRepository> mockCouponsRepository;
        private readonly Mock<IUserCouponsRepository> mockUserCouponsRepository;
        private readonly Mock<IProductsService> mockProductsService;
        private readonly Mock<IServicesHub> mockServicesHub;
        private readonly IMapper mapper;

        public CouponsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockCouponsRepository = mockRepository.Create<ICouponsRepository>();
            mockUserCouponsRepository = mockRepository.Create<IUserCouponsRepository>();
            mockProductsService = mockRepository.Create<IProductsService>();
            mockServicesHub = mockRepository.Create<IServicesHub>();
            mockServicesHub.SetupGet(s => s.ProductsService).Returns(mockProductsService.Object);
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
        }

        [Fact]
        public async Task GetCoupons_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new List<Coupon>
            {
                new Coupon
                {
                    TenantId = user!.User!.TenantId
                }
            };

            mockCouponsRepository.Setup(r => r.GetAllAsync(user!.User!.TenantId)).ReturnsAsync(expected);

            // Act
            var result = await service.GetAllAsync(user);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?.First().TenantId.Should().Be(user!.User!.TenantId);
            mockCouponsRepository.Verify(r => r.GetAllAsync(user!.User!.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetCouponByFilterLogged_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new List<Coupon>
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

            mockCouponsRepository.Setup(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId, true))
                .ReturnsAsync(new PaginatedResult<Coupon> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CouponDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(3);
            result?.Itens.TrueForAll(p => expected.Select(e => e.Id).Contains(p.Id));
            mockCouponsRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCouponsByFilter_NotFound_Test()
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
            var result = await service.GetByFilterAsync(filter, testUser!);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<PaginatedResult<CouponDto>>();
            result?.Itens.Should().NotBeNull();
            result?.Itens.Should().HaveCount(0);
            mockCouponsRepository.Verify(r => r.GetByFilterAsync(filter, testUser!.User!.TenantId, true), Times.Once);
        }

        [Fact]
        public async Task GetCoupon_Test()
        {
            // Arrange
            var service = CreateService();

            var user = GetTestUser();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = user!.User!.TenantId,
                Key = "COUPON",
                Discount = 10
            };

            mockCouponsRepository.Setup(r => r.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            // Act
            var result = await service.GetByIdAsync(expected.Id, user);

            // Assert
            result?.Should().NotBeNull();
            result?.Id.Should().Be(expected.Id);
            result?.TenantId.Should().Be(user!.User!.TenantId);
            mockCouponsRepository.Verify(r => r.GetByIdAsync(expected.Id), Times.Once);
        }

        [Fact]
        public async Task SaveCoupon_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId
            };

            var input = new CouponDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId
            };

            mockCouponsRepository.Setup(r => r.SaveAsync(It.IsAny<Coupon>(), string.Empty))
               .ReturnsAsync(expected);

            // Act
            var result = await service.SaveAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CouponDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCouponsRepository.Verify(r => r.SaveAsync(It.IsAny<Coupon>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task CreateCoupon_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId,
                Key = "COUPON",
                Discount = 10
            };

            var input = new CouponDto
            {
                Id = string.Empty,
                TenantId = testUser!.User!.TenantId,
                Key = "COUPON",
                Discount = 10
            };

            mockCouponsRepository.Setup(r => r.SaveAsync(It.IsAny<Coupon>(), testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.CreateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CouponDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCouponsRepository.Verify(r => r.SaveAsync(It.IsAny<Coupon>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCoupon_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId,
                Key = "COUPON",
                Discount = 10
            };

            var input = new CouponDto
            {
                Id = expected.Id,
                TenantId = testUser!.User!.TenantId,
                Key = "COUPON",
                Discount = 10
            };

            mockCouponsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCouponsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
               .ReturnsAsync(expected);

            // Act
            var result = await service.UpdateAsync(input, testUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<CouponDto>();
            result?.TenantId.Should().Be(expected.TenantId);
            mockCouponsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
            mockCouponsRepository.Verify(r => r.SaveAsync(It.IsAny<Coupon>(), testUser!.User!.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateCouponUnauthorized_Test()
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser!.User!.Permissions.Clear();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId ?? string.Empty,
                Key = "COUPON",
                Discount = 10
            };

            var input = new CouponDto
            {
                Id = expected.Id,
                TenantId = "22222222222222",
                Key = "COUPON",
                Discount = 10
            };

            mockCouponsRepository.Setup(r => r.GetByIdAsync(input.Id))
               .ReturnsAsync(expected);

            mockCouponsRepository.Setup(r => r.SaveAsync(expected, testUser!.User!.Id))
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
            mockCouponsRepository.Verify(r => r.GetByIdAsync(input.Id), Times.Never);
            mockCouponsRepository.Verify(r => r.SaveAsync(It.IsAny<Coupon>(), testUser!.User!.Id), Times.Never);
        }

        [Theory]
        [InlineData("MISSING")]
        [InlineData("MISSING_PRODUCT")]
        [InlineData("PRODUCT")]
        [InlineData("CATEGORY")]
        [InlineData("USER_LIMIT")]
        [InlineData("COUPON_LIMIT")]
        [InlineData("EXPIRED")]
        [InlineData("NOT_STARTED")]
        [InlineData("VALID")]
        public async Task ValidateCoupon(string key)
        {
            // Arrange
            var service = CreateService();

            var testUser = GetTestUser();
            testUser!.User!.Permissions.Clear();

            var expected = new Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = testUser!.User!.TenantId ?? string.Empty,
                Key = "COUPON",
                Discount = 10,
                UsesPerUser = 0,
                Quantity = 0
            };

            var request = new CouponValidationRequest
            {
                TenantId = testUser!.User!.TenantId ?? string.Empty,
                Coupon = key,
                UserId = testUser!.User!.Id,
                ProductId = "123456789"
            };

            mockProductsService.Setup(r => r.GetByIdAsync(request.ProductId))
                .ReturnsAsync(new ProductDto
                {
                    Id = request.ProductId,
                    TenantId = testUser!.User!.TenantId,

                    Categories = ["CATEGORY"]
                });

            mockCouponsRepository.Setup(r => r.GetByKey(request.TenantId, request.Coupon))
               .ReturnsAsync(expected);

            if (expected != null)
            {
                mockUserCouponsRepository.Setup(r => r.GetByCouponId(request.TenantId, expected.Id))
                   .ReturnsAsync(new List<UserCoupon> {
                       new UserCoupon
                        {
                            Id = Guid.NewGuid().ToString(),
                            CouponId = expected.Id,
                            UserId = testUser!.User!.Id,
                            TenantId = testUser!.User!.TenantId ?? string.Empty
                        }
                   });
            }

            switch (key)
            {
                case "MISSING":
                    request.Coupon = "OTHER";
                    break;

                case "MISSING_PRODUCT":
                    request.ProductId = "987654321";
                    break;

                case "PRODUCT":
                    expected.Products = new List<string>() { "123456" };
                    break;

                case "CATEGORY":
                    expected.Categories = new List<string>() { "MISSING" };
                    break;

                case "USER_LIMIT":
                    expected.UsesPerUser = 1;
                    break;

                case "COUPON_LIMIT":
                    expected.Quantity = 1;
                    break;

                case "EXPIRED":
                    expected.EndDate = DateTime.Now.AddDays(-1);
                    break;

                case "NOT_STARTED":
                    expected.StartDate = DateTime.Now.AddDays(1);
                    break;
            }

            // Act
            var response = await service.ValidateCouponAsync(request);

            // Assert
            switch (key)
            {
                case "MISSING":
                    response.Code.Should().Be(CouponValidationResponseCodes.InvalidCoupon);
                    break;

                case "MISSING_PRODUCT":
                    response.Code.Should().Be(CouponValidationResponseCodes.MissingProduct);
                    break;

                case "PRODUCT":
                    response.Code.Should().Be(CouponValidationResponseCodes.InvalidProduct);
                    break;

                case "CATEGORY":
                    response.Code.Should().Be(CouponValidationResponseCodes.InvalidCategory);
                    break;

                case "USER_LIMIT":
                    response.Code.Should().Be(CouponValidationResponseCodes.UserLimitReached);
                    break;

                case "COUPON_LIMIT":
                    response.Code.Should().Be(CouponValidationResponseCodes.CouponLimitReached);
                    break;

                case "EXPIRED":
                    response.Code.Should().Be(CouponValidationResponseCodes.Expired);
                    break;

                case "NOT_STARTED":
                    response.Code.Should().Be(CouponValidationResponseCodes.NotStarted);
                    break;

                case "VALID":
                    response.Code.Should().Be(CouponValidationResponseCodes.Valid);
                    response.CouponId = expected.Id;
                    break;
            }
        }

        private CouponsService CreateService()
        {
            return new CouponsService(
                mockCouponsRepository.Object,
                mockUserCouponsRepository.Object,
                mockServicesHub.Object,
                mapper);
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
                Permissions = ["admin", "Coupons_admin", "staff"],
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