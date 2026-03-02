using Microsoft.AspNetCore.Mvc;
using EHive.Authorization.Library.Extensions;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Search;
using EHive.WebExtensions.Library;
using EHive.Search.Domain.Abstractions.Services;
using EHive.Configuration.Library.Models;

namespace EHive.Search.Api.Endpoints
{
    internal static class ProductCourseSearchEndpoints
    {
        internal static WebApplication MapProductCourseSearchEndpoints(this WebApplication app)
        {
            app.MapGet("v1/ProductCourse/{tenantId}", async (HttpContext context, [FromServices] IProductCourseSearchService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, tenantId);
                return Results.Ok(Response<PaginatedResult<ProductCourseSearchDto>>.Ok(result));
            })
            .WithName("GetProductCourseOpen")
            .WithDescription("Product Course open")
            .WithTags("ProductCourse")
            .Produces<Response<PaginatedResult<ProductCourseSearchDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/ProductCourse/FilterScope/{tenantId}", async (HttpContext context, [FromServices] IProductCourseSearchService service, [FromRoute] string tenantId) =>
            {
                var result = await service.GetFilterScopeAsync(tenantId);
                return Results.Ok(Response<FilterScope>.Ok(result));
            })
            .WithName("GetProductsCourses Filter Scope")
            .WithDescription("Get filter scope")
            .WithTags("ProductCourse")
            .Produces<Response<FilterScope>>()
            .AllowAnonymous();

            app.MapGet("v1/ProductCourse/Trigger", async (HttpContext context, [FromServices] IProductCourseSearchService service, [FromQuery] bool full = false) =>
            {
                var result = await service.TriggerGathering(full);
                return Results.Ok(Response<int>.Ok(result));
            })
            .WithName("TriggerGathering")
            .WithDescription("Product Course")
            .WithTags("ProductCourse")
            .WithMetadata(PermissionConfig.Create("search_admin"))
            .Produces<Response<int>>();

            return app;
        }
    }
}