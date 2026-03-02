namespace EHive.Core.Library.Abstractions.Enrich
{
    public interface IEnrichable
    {
        string Id { get; set; }

        string VId { get; set; }

        string TenantId { get; set; }

        Dictionary<string, object> CustomAttributes { get; set; }
    }
}