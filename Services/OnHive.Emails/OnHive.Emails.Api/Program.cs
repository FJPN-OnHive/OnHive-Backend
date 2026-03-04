using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.Extensions;
using OnHive.Emails.Api.DependencyInjection;
using OnHive.Emails.Api.Endpoints;
using OnHive.Emails.Api.Middlewares;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.HealthCheck.Library.Extensions;
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