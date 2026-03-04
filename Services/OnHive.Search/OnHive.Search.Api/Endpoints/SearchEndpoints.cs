using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Search;
using OnHive.Search.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;

namespace OnHive.Search.Api.Endpoints
{
    internal static class SearchEndpoints
    {
        internal static WebApplication MapSearchEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Search/{tenantId}", async (HttpContext context, [FromServices] ISearchService service, [FromRoute] string tenantId) =>
            {
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, tenantId);
                return Results.Ok(Response<List<SearchResultDto>>.Ok(result));
            })
            .WithName("GetSearchOpen")
            .WithDescription("Search open")
            .WithTags("Search")
            .Produces<Response<List<SearchResultDto>>>()
            .AllowAnonymous();

            app.MapGet("v1/Search", async (HttpContext context, [FromServices] ISearchService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser?.User == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<List<SearchResultDto>>.Ok(result));
            })
            .WithName("GetSearch")
            .WithDescription("Search")
            .WithTags("Search")
            .WithMetadata(PermissionConfig.Create("search_read"))
            .Produces<Response<List<SearchResultDto>>>();

            app.MapGet("v1/Search/Trigger", async (HttpContext context, [FromServices] ISearchService service, [FromQuery] bool full = false) =>
            {
                var result = await service.TriggerGathering(full);
                return Results.Ok(Response<int>.Ok(result));
            })
           .WithName("TriggerSearchGathering")
           .WithDescription("Search Trigger")
           .WithTags("Search")
           .WithMetadata(PermissionConfig.Create("search_admin"))
           .Produces<Response<int>>();

            return app;
        }
    }
}