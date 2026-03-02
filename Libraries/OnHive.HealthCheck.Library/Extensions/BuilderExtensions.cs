using EHive.HealthCheck.Library.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EHive.HealthCheck.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureHealthChecks(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IHealthCheckCollection, HealthCheckCollection>();
            return builder;
        }
    }
}