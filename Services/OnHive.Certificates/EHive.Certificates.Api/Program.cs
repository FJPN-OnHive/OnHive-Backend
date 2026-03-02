using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
using EHive.Certificates.Api.DependencyInjection;
using EHive.Certificates.Api.Middlewares;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureCertificatesApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapCertificatesApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();