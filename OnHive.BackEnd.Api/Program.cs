using EHive.Authorization.Library.Extensions;
using EHive.Backend.Api.Middlewaress;
using EHive.Catalog.Api.DependencyInjection;
using EHive.Certificates.Api.DependencyInjection;
using EHive.Configuration.Api.DependencyInjection;
using EHive.Configuration.Library.Extensions;
using EHive.Courses.Api.DependencyInjection;
using EHive.Database.Library.Extensions;
using EHive.Dict.Api.DependencyInjection;
using EHive.Emails.Api.DependencyInjection;
using EHive.Events.Api.DependencyInjection;
using EHive.HealthCheck.Library.Extensions;
using EHive.Invoices.Api.DependencyInjection;
using EHive.Messages.Api.DependencyInjection;
using EHive.Orders.Api.DependencyInjection;
using EHive.Payments.Api.DependencyInjection;
using EHive.PaymentsCielo.Api.DependencyInjection;
using EHive.Posts.Api.DependencyInjection;
using EHive.Redirects.Api.DependencyInjection;
using EHive.Search.Api.DependencyInjection;
using EHive.Storages.Api.DependencyInjection;
using EHive.Students.Api.DependencyInjection;
using EHive.SystemParameters.Api.DependencyInjection;
using EHive.Teachers.Api.DependencyInjection;
using EHive.Tenants.Api.DependencyInjection;
using EHive.Users.Api.DependencyInjection;
using EHive.Videos.Api.DependencyInjection;
using OnHive.Domains.Common.Extensions;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurationSources();
builder.ConfigureHealthChecks();
builder.ConfigureAuthentication();
builder.ConfigureMongoDb();
builder.ConfigureCors();
builder.ConfigureOpenApi();

builder.ConfigureEventsApi();
builder.ConfigureTenantsApi();
builder.ConfigureUsersApi();
builder.ConfigureConfigurationApi();
builder.ConfigureSystemParametersApi();
builder.ConfigureEmailsApi();
builder.ConfigureVideosApi();
builder.ConfigureDictApi();
builder.ConfigureCertificatesApi();
builder.ConfigureProductsApi();
builder.ConfigureStoragesApi();
builder.ConfigureCoursesApi();
builder.ConfigureMessagesApi();
builder.ConfigureOrdersApi();
builder.ConfigurePaymentsApi();
builder.ConfigureCieloPaymentsApi();
builder.ConfigurePostsApi();
builder.ConfigureRedirectApi();
builder.ConfigureSearchApi();
builder.ConfigureStudentsApi();
builder.ConfigureTeachersApi();
builder.ConfigureInvoicesApi();

builder.Services.SetServiceProviderHelper();
builder.Services.SetServicesHub();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapFallbackToFile("index.html");

app.MapConfigurationApi();
app.MapTenantsApi();
app.MapUsersApi();
app.MapSystemParametersApi();
app.MapEmailsApi();
app.MapStoragesApi();
app.MapVideosApi();
app.MapCatalogApi();
app.MapCoursesApi();
app.MapCertificatesApi();
app.MapDictApi();
app.MapEventsApi();
app.MapInvoicesApi();
app.MapMessagesApi();
app.MapOrdersApi();
app.MapPaymentsApi();
app.MapPostsApi();
app.MapRedirectsApi();
app.MapSearchApi();
app.MapStudentsApi();
app.MapTeachersApi();

app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();