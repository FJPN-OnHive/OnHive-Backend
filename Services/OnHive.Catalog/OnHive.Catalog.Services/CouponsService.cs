using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Repositories;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Core.Library.Enums.Catalog;
using OnHive.Core.Library.Validations.Common;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;

namespace OnHive.Catalog.Services
{
    public class CouponsService : ICouponsService
    {
        private readonly ICouponsRepository couponsRepository;
        private readonly IUserCouponsRepository userCouponsRepository;
        private readonly IProductsService productsService;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public CouponsService(ICouponsRepository couponsRepository,
                              IUserCouponsRepository userCouponsRepository,
                              IServicesHub servicesHub,
                              IMapper mapper)
        {
            this.couponsRepository = couponsRepository;
            this.userCouponsRepository = userCouponsRepository;
            this.productsService = servicesHub.ProductsService;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<CouponDto?> GetByIdAsync(string CouponId)
        {
            var Coupon = await couponsRepository.GetByIdAsync(CouponId);
            var result = mapper.Map<CouponDto>(Coupon);
            return result;
        }

        public async Task<CouponDto?> GetByIdAsync(string CouponId, LoggedUserDto? loggedUser)
        {
            var Coupon = await couponsRepository.GetByIdAsync(CouponId);
            var result = mapper.Map<CouponDto>(Coupon);
            return result;
        }

        public async Task<List<UserCouponDto>> GetUsesByIdAsync(string couponId, LoggedUserDto loggedUser)
        {
            if (string.IsNullOrEmpty(loggedUser?.User?.TenantId)) throw new ArgumentException(nameof(loggedUser.User.TenantId));
            var result = await userCouponsRepository.GetByCouponId(loggedUser.User.TenantId, couponId);
            return mapper.Map<List<UserCouponDto>>(result);
        }

        public async Task<PaginatedResult<CouponDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            if (string.IsNullOrEmpty(loggedUser?.User?.TenantId)) throw new ArgumentException(nameof(loggedUser.User.TenantId));
            var result = await couponsRepository.GetByFilterAsync(filter, loggedUser.User.TenantId);
            if (result != null)
            {
                return new PaginatedResult<CouponDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<CouponDto>>(result.Itens)
                };
            }
            return new PaginatedResult<CouponDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<CouponDto>()
            };
        }

        public async Task<IEnumerable<CouponDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var Coupons = await couponsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<CouponDto>>(Coupons);
        }

        public async Task<CouponDto> SaveAsync(CouponDto CouponDto, LoggedUserDto? loggedUser)
        {
            var Coupon = mapper.Map<Coupon>(CouponDto);
            ValidatePermissions(Coupon, loggedUser?.User);
            Coupon.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            Coupon.CreatedAt = DateTime.UtcNow;
            Coupon.CreatedBy = string.IsNullOrEmpty(Coupon.CreatedBy) ? loggedUser.User.Id : Coupon.CreatedBy;

            var response = await couponsRepository.SaveAsync(Coupon);
            return mapper.Map<CouponDto>(response);
        }

        public async Task<CouponDto> CreateAsync(CouponDto CouponDto, LoggedUserDto? loggedUser)
        {
            if (!CouponDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var Coupon = mapper.Map<Coupon>(CouponDto);
            ValidatePermissions(Coupon, loggedUser?.User);
            Coupon.Id = string.Empty;
            Coupon.TenantId = loggedUser?.User?.TenantId ?? string.Empty;
            var response = await couponsRepository.SaveAsync(Coupon, loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser.User.Id)));
            return mapper.Map<CouponDto>(response);
        }

        public async Task<CouponDto?> UpdateAsync(CouponDto CouponDto, LoggedUserDto? loggedUser)
        {
            if (!CouponDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var Coupon = mapper.Map<Coupon>(CouponDto);
            ValidatePermissions(Coupon, loggedUser?.User);
            var currentCoupon = await couponsRepository.GetByIdAsync(Coupon.Id);
            if (currentCoupon == null || currentCoupon.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await couponsRepository.SaveAsync(Coupon, loggedUser.User.Id);
            return mapper.Map<CouponDto>(response);
        }

        public async Task<CouponValidationResponse> ValidateCouponAsync(CouponValidationRequest couponValidationRequest)
        {
            if (string.IsNullOrEmpty(couponValidationRequest.TenantId)) throw new ArgumentException(nameof(couponValidationRequest.TenantId));
            var coupon = await couponsRepository.GetByKey(couponValidationRequest.TenantId, couponValidationRequest.Coupon);
            if (coupon == null)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Coupon not found",
                    CouponId = string.Empty,
                    Code = CouponValidationResponseCodes.InvalidCoupon
                };
            }
            var product = await productsService.GetByIdAsync(couponValidationRequest.ProductId);
            if (product == null)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Product not found",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.MissingProduct
                };
            }
            if (coupon.Products.Any() && !coupon.Products.Contains(couponValidationRequest.ProductId))
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Coupon not valid for this product",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.InvalidProduct
                };
            }
            if (coupon.Categories.Any() && !coupon.Categories.TrueForAll(c => product.Categories.Contains(c)))
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Coupon not valid for this category",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.InvalidCategory
                };
            }
            var userCoupons = await userCouponsRepository.GetByCouponId(couponValidationRequest.TenantId, coupon.Id);
            var userCouponsByUser = userCoupons.Where(uc => uc.UserId == couponValidationRequest.UserId).ToList();
            if (coupon.UsesPerUser > 0 && userCouponsByUser.Count() >= coupon.UsesPerUser)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = $"Coupon reached it's limit for the user ({coupon.UsesPerUser})",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.UserLimitReached
                };
            }
            if (coupon.Quantity > 0 && userCoupons.Count() >= coupon.Quantity)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = $"Coupon reached it's limit for the coupon ({coupon.Quantity})",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.CouponLimitReached
                };
            }
            if (coupon.EndDate < DateTime.Now)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Coupon expired",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.Expired
                };
            }
            if (coupon.StartDate > DateTime.Now)
            {
                return new CouponValidationResponse
                {
                    IsValid = false,
                    Message = "Coupon not started",
                    CouponId = coupon.Id,
                    Code = CouponValidationResponseCodes.NotStarted
                };
            }
            return new CouponValidationResponse
            {
                IsValid = true,
                Message = "Coupon is valid",
                CouponId = coupon.Id,
                Code = CouponValidationResponseCodes.Valid
            };
        }

        public async Task<CouponValidationResponse> ApplyCouponAsync(CouponApplyRequest couponApplyRequest)
        {
            var validate = await ValidateCouponAsync(couponApplyRequest);
            if (!validate.IsValid)
            {
                return validate;
            }
            var userCon = new UserCoupon
            {
                TenantId = couponApplyRequest.TenantId,
                CouponId = validate.CouponId,
                UserId = couponApplyRequest.UserId,
                ProductId = couponApplyRequest.ProductId,
                OrderId = couponApplyRequest.OrderId,
                Date = DateTime.UtcNow,
                CreatedBy = couponApplyRequest.UserId,
                CreatedAt = DateTime.UtcNow
            };
            await userCouponsRepository.SaveAsync(userCon, couponApplyRequest.UserId);
            return validate;
        }

        private void ValidatePermissions(Coupon Coupon, UserDto? loggedUser)
        {
            if (loggedUser != null && Coupon.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Coupon/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    Coupon.Id, Coupon.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}