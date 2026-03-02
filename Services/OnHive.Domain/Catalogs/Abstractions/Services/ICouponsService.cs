using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;

namespace EHive.Catalog.Domain.Abstractions.Services
{
    public interface ICouponsService
    {
        Task<CouponDto?> GetByIdAsync(string CouponId);

        Task<CouponDto?> GetByIdAsync(string CouponId, LoggedUserDto? loggedUser);

        Task<PaginatedResult<CouponDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<CouponDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<CouponDto> SaveAsync(CouponDto CouponDto, LoggedUserDto? user);

        Task<CouponDto> CreateAsync(CouponDto CouponDto, LoggedUserDto loggedUser);

        Task<CouponDto?> UpdateAsync(CouponDto CouponDto, LoggedUserDto loggedUser);

        Task<CouponValidationResponse> ValidateCouponAsync(CouponValidationRequest couponValidationRequest);

        Task<CouponValidationResponse> ApplyCouponAsync(CouponApplyRequest couponApplyRequest);

        Task<List<UserCouponDto>> GetUsesByIdAsync(string couponId, LoggedUserDto loggedUser);
    }
}