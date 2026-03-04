using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Users.Api.DependencyInjection;
using OnHive.Users.Api.Middlewares;
using OnHive.Users.Domain.Abstractions.Services;
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
Log.Information("Starting OnHive Users API");
app.Run();