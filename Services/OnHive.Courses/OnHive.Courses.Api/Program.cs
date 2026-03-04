using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Extensions;
using OnHive.Courses.Api.DependencyInjection;
using OnHive.Courses.Api.Middlewares;
using OnHive.Database.Library.Extensions;
using OnHive.HealthCheck.Library.Extensions;
using Serilog;
using System.Text.Json.Serialization;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureCors();
builder.ConfigureCoursesApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.AddHealthChecks();
app.MapCoursesApi();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();