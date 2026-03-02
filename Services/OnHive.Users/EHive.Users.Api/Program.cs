using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Configuration.Library.Models;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
using EHive.Users.Api.DependencyInjection;
using EHive.Users.Api.Middlewares;
using EHive.Users.Domain.Abstractions.Services;
using Serilog;
using OnHive.Domains.Common.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogger("Users.Api");
builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureUsersApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

await app.Services
    .GetRequiredService<IRolesService>()
    .Migrate(app.Services.GetRequiredService<EnvironmentSettings>().EnvironmentType?.Equals("prod", StringComparison.InvariantCultureIgnoreCase) ?? false);

await app.Services
    .GetRequiredService<IUsersService>()
    .Migrate(app.Services.GetRequiredService<EnvironmentSettings>().EnvironmentType?.Equals("prod", StringComparison.InvariantCultureIgnoreCase) ?? false);

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapUsersApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
Log.Information("Starting EHive Users API");
app.Run();