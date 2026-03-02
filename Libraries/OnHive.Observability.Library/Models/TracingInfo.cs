namespace EHive.Observability.Library.Models
{
    public class TracingInfo
    {
        public string? TracingId { get; set; }

        public string? PreviousTracingId { get; set; }

        public string? Origin { get; set; }

        public string? User { get; set; }

        public string? UserId { get; set; }

        public string? Tenant { get; set; }

        public string? TenantId { get; set; }

        public static string NewTracingId()
        {
            var tracingString = Guid.NewGuid().ToString();
            return tracingString.Replace("-", "");
        }
    }
}