using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Redirects.Api.DependencyInjection;
using OnHive.Redirects.Api.Middlewares;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureRedirectApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapRedirectsApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();