using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Payments.Api.DependencyInjection;
using OnHive.Payments.Api.Middlewares;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigurePaymentsApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapPaymentsApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();