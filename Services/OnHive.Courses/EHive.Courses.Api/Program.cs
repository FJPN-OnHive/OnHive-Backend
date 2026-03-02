using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.Courses.Api.DependencyInjection;
using EHive.Courses.Api.Middlewares;
using EHive.Database.Library.Extensions;
using EHive.HealthCheck.Library.Extensions;
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