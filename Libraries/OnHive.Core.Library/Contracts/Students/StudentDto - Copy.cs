using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Students
{
    public class StudentReportDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("reportName")]
        public string ReportName { get; set; } = string.Empty;

        [JsonPropertyName("reportDate")]
        public DateTime ReportDate { get; set; }

        [JsonPropertyName("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;
    }
}