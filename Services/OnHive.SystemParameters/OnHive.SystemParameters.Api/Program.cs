using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.SystemParameters.Api.DependencyInjection;
using OnHive.SystemParameters.Api.Middlewares;
using OnHive.SystemParameters.Domain.Abstractions.Services;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureSystemParametersApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

await app.Services
    .GetRequiredService<ISystemParametersService>()
    .Migrate();

app.AddSwagger();
app.AddHealthChecks();
app.MapSystemParametersApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();