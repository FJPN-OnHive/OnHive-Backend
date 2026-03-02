using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
using EHive.SystemParameters.Api.DependencyInjection;
using EHive.SystemParameters.Api.Middlewares;
using EHive.SystemParameters.Domain.Abstractions.Services;
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