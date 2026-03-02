using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
using EHive.Tenants.Api.DependencyInjection;
using EHive.Tenants.Api.Middlewares;
using EHive.Tenants.Domain.Abstractions.Services;
using Serilog;
using System.Text.Json.Serialization;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureTenantsApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

var app = builder.Build();

await app.Services
    .GetRequiredService<IFeaturesService>()
    .Migrate();

await app.Services
    .GetRequiredService<ITenantsService>()
    .Migrate(app.Services.GetRequiredService<EnvironmentSettings>().EnvironmentType?.Equals("prod", StringComparison.InvariantCultureIgnoreCase) ?? false);

app.AddCorsCofiguration();
app.AddHealthChecks();
app.MapTenantsApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
Log.Information("Starting EHive Tenants API");
app.Run();