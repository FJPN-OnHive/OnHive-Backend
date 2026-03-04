using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Messages.Api.DependencyInjection;
using OnHive.Domains.Common.Extensions;
using OnHive.Messages.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureMessagesApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapMessagesApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();