using Microsoft.AspNetCore.Mvc;
using OnHive.Authorization.Library.Extensions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Search;
using OnHive.WebExtensions.Library;
using OnHive.Search.Domain.Abstractions.Services;
using OnHive.Configuration.Library.Models;

namespace OnHive.Search.Api.Endpoints
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