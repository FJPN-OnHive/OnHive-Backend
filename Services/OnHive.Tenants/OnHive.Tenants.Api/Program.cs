using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Tenants.Api.DependencyInjection;
using OnHive.Tenants.Api.Middlewares;
using OnHive.Tenants.Domain.Abstractions.Services;
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
Log.Information("Starting OnHive Tenants API");
app.Run();