using EHive.Authorization.Library.Extensions;
using EHive.Configuration.Library.Extensions;
using EHive.PaymentsCielo.Api.DependencyInjection;
using EHive.PaymentsCielo.Api.Endpoints;
using EHive.PaymentsCielo.Api.Middlewares;
using OnHive.Domains.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureAuthentication();
builder.ConfigureCieloPaymentsApi();
builder.ConfigureCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();
builder.Services.SetServiceProviderHelper();

var app = builder.Build();

app.AddCorsCofiguration();
app.AddSwagger();
app.MapPaymentCieloEndpoints();
app.AddAuthentication();
app.AddTracing();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();