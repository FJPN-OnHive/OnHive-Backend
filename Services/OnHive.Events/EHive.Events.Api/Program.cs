using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
using EHive.Events.Api.DependencyInjection;
using OnHive.Domains.Common.Extensions;
using EHive.Events.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogger("Envents.Api");
builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureEventsApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapEventsApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();