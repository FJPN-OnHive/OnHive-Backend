using Serilog.Events;

namespace OnHive.Observability.Library.Models
{
    public class LoggingSettings
    {
        public bool EnableLog { get; set; } = true;

        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

        public string LogGroup { get; set; } = string.Empty;

        public string OpenTelemetryUrl { get; set; } = "http://opentelemetry-collector:4317";
    }
}