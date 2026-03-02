using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Tenants.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace EHive.Tenants.Api.Endpoints
{
    internal static class FeaturesEndpoints
    {
        internal static WebApplication MapFeaturesEndpoints(this WebApplication app)
        {
            app.MapGet("v1/Features", async (HttpContext context, [FromServices] IFeaturesService service) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var result = await service.GetAllAsync();
                if (result == null || !result.Any()) return Results.Ok(Response<IEnumerable<FeatureDto>>.Empty());
                return Results.Ok(Response<IEnumerable<FeatureDto>>.Ok(result));
            })
            .WithName("GetFeatures")
            .WithDescription("Get features by Id")
            .WithTags("Features")
            .WithMetadata(PermissionConfig.Create("tenants_read"))
            .Produces<Response<IEnumerable<FeatureDto>>>();

            return app;
        }
    }
}