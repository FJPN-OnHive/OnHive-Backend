namespace EHive.Core.Library.Entities.Certificates
{
    public class Certificate : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string TemplateUrl { get; set; } = string.Empty;

        public string TemplateBody { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public List<CertificateParameter> Parameters { get; set; } = new();
    }
}