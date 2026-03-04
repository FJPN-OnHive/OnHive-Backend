using OnHive.Observability.Library.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace OnHive.Configuration.Library.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureLogger(this WebApplicationBuilder builder, string serviceName)
        {
            builder.AddConfiguration<LoggingSettings>();
            ConfigureLogger(builder.Services, serviceName);
            return builder;
        }

        private static void ConfigureLogger(IServiceCollection services, string serviceName)
        {
            var logSettings = services.BuildServiceProvider().GetRequiredService<LoggingSettings>();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("OnHive", logSettings.LogLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service", serviceName)
                .Enrich.WithProperty("environment", logSettings.LogGroup)
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {SourceContext}{NewLine}{Exception}")
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = logSettings.OpenTelemetryUrl;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        { "service.name", serviceName ?? "OnHiveService" },
                        { "deployment.environment", logSettings.LogGroup }
                    };
                })
                .CreateLogger();
        }
    }
}