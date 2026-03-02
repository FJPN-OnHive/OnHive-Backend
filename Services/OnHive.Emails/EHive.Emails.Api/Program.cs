using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Database.Library.Extensions;
using EHive.Emails.Api.DependencyInjection;
using EHive.Emails.Api.Endpoints;
using EHive.Emails.Api.Middlewares;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.HealthCheck.Library.Extensions;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureEmailsApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

await app.Services
    .GetRequiredService<IEmailsService>()
    .Migrate();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapEmailsEndpoints();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();