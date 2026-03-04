using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace OnHive.HealthCheck.Library.Helpers
{
    internal static class HealthCheckHelpers
    {
        public static string SerializerResult(List<HealthCheckResult> healthCheckResults)
        {
            var result = new { Results = new List<object>() };

            foreach (var item in healthCheckResults)
            {
                result.Results.Add(new
                {
                    Status = item.Status.ToString(),
                    Description = item.Description ?? "",
                    Message = item.Exception?.Message ?? "",
                });
            }

            return JsonSerializer.Serialize(result);
        }

        public static JsonDocument SerializerResultToDocument(List<HealthCheckResult> healthCheckResults)
        {
            return JsonDocument.Parse(SerializerResult(healthCheckResults));
        }
    }
}