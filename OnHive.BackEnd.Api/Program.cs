using OnHive.Authorization.Library.Extensions;
using OnHive.Backend.Api.Middlewaress;
using OnHive.Catalog.Api.DependencyInjection;
using OnHive.Certificates.Api.DependencyInjection;
using OnHive.Configuration.Api.DependencyInjection;
using OnHive.Configuration.Library.Extensions;
using OnHive.Courses.Api.DependencyInjection;
using OnHive.Database.Library.Extensions;
using OnHive.Dict.Api.DependencyInjection;
using OnHive.Emails.Api.DependencyInjection;
using OnHive.Events.Api.DependencyInjection;
using OnHive.HealthCheck.Library.Extensions;
using OnHive.Invoices.Api.DependencyInjection;
using OnHive.Messages.Api.DependencyInjection;
using OnHive.Orders.Api.DependencyInjection;
using OnHive.Payments.Api.DependencyInjection;
using OnHive.PaymentsCielo.Api.DependencyInjection;
using OnHive.Posts.Api.DependencyInjection;
using OnHive.Redirects.Api.DependencyInjection;
using OnHive.Search.Api.DependencyInjection;
using OnHive.Storages.Api.DependencyInjection;
using OnHive.Students.Api.DependencyInjection;
using OnHive.SystemParameters.Api.DependencyInjection;
using OnHive.Teachers.Api.DependencyInjection;
using OnHive.Tenants.Api.DependencyInjection;
using OnHive.Users.Api.DependencyInjection;
using OnHive.Videos.Api.DependencyInjection;
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