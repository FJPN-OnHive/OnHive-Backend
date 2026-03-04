using OnHive.Authorization.Library.Extensions;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace OnHive.Catalog.Api.Endpoints
{
    internal static class CouponsEndpoints
    {
        internal static WebApplication MapCouponsEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Coupon/{CouponId}", async (HttpContext context, [FromServices] ICouponsService service, [FromRoute] string CouponId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetByIdAsync(CouponId, loggedUser);
                if (result == null) return Results.Ok(Response<CouponDto>.Empty());
                return Results.Ok(Response<CouponDto>.Ok(result));
            })
            .WithName("GetCouponById")
            .WithDescription("Get Coupon by Id")
            .WithTags("Coupons")
            .WithMetadata(PermissionConfig.Create("Coupons_read"))
            .Produces<Response<CouponDto>>();

            app.MapGet("v1/Coupon/Uses/{CouponId}", async (HttpContext context, [FromServices] ICouponsService service, [FromRoute] string CouponId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetUsesByIdAsync(CouponId, loggedUser);
                if (result == null) return Results.Ok(Response<List<UserCouponDto>>.Empty());
                return Results.Ok(Response<List<UserCouponDto>>.Ok(result));
            })
            .WithName("GetCouponUsesById")
            .WithDescription("Get Coupon Uses by Id")
            .WithTags("Coupons")
            .WithMetadata(PermissionConfig.Create("Coupons_read"))
            .Produces<Response<PaginatedResult<UserCouponDto>>>();

            app.MapPost("v1/Coupon/Validate", async (HttpContext context, [FromServices] ICouponsService service, [FromBody] CouponValidationRequest couponValidationRequest) =>
            {
                var result = await service.ValidateCouponAsync(couponValidationRequest);
                return Results.Ok(Response<CouponValidationResponse>.Ok(result));
            })
            .WithName("PostCouponValidate")
            .WithDescription("Validate Coupon")
            .WithTags("Coupons")
            .Produces<Response<CouponValidationResponse>>()
            .AllowAnonymous();

            //app.MapPost("v1/Internal/Coupon/Apply", async (HttpContext context, [FromServices] ICouponsService service, [FromBody] CouponApplyRequest couponApplyRequest) =>
            //{
            //    var result = await service.ApplyCouponAsync(couponApplyRequest);
            //    return Results.Ok(Response<CouponValidationResponse>.Ok(result));
            //})
            //.WithName("PostCouponApply")
            //.WithDescription("Apply Coupon")
            //.WithTags("Internal")
            //.Produces<Response<CouponValidationResponse>>();

            app.MapGet("v1/Coupons", async (HttpContext context, [FromServices] ICouponsService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<CouponDto>>.Ok(result));
            })
            .WithName("GetCoupons")
            .WithDescription("Get all Coupons")
            .WithTags("Coupons")
            .WithMetadata(PermissionConfig.Create("Coupons_read"))
            .Produces<Response<PaginatedResult<CouponDto>>>();

            app.MapPost("v1/Coupon", async (HttpContext context, [FromServices] ICouponsService service, [FromBody] CouponDto CouponDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.CreateAsync(CouponDto, loggedUser);
                if (result == null) return Results.Ok(Response<CouponDto>.Empty());
                return Results.Ok(Response<CouponDto>.Ok(result));
            })
            .WithName("CreateCoupon")
            .WithDescription("Create an Coupon")
            .WithTags("Coupons")
            .WithMetadata(PermissionConfig.Create("Coupons_create"))
            .Produces<Response<CouponDto>>();

            app.MapPut("v1/Coupon", async (HttpContext context, [FromServices] ICouponsService service, [FromBody] CouponDto CouponDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(CouponDto, loggedUser);
                if (result == null) return Results.Ok(Response<CouponDto>.Empty());
                return Results.Ok(Response<CouponDto>.Ok(result));
            })
            .WithName("UpdateCoupon")
            .WithDescription("Update an Coupon")
            .WithTags("Coupons")
            .WithMetadata(PermissionConfig.Create("Coupons_update"))
            .Produces<Response<CouponDto>>();

            return app;
        }
    }
}