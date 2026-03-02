namespace EHive.Events.Domain.Abstractions.Services
{
    public interface IEventRegister
    {
        Task RegisterEvent(string tenantId, string userId, string key, string message, Dictionary<string, string> fields);

        Task RegisterEvent(string tenantId, string userId, string key, string message, Dictionary<string, string> fields, List<string> tags);

        Task RegisterEvent(string tenantId, string userId, string key, string message, List<string> fields, List<string> values, List<string> tags);

        Task RegisterEvent(string tenantId, string userId, string key, string message, List<string> fields, params string[] values);

        Task RegisterEvent(string tenantId, string userId, string key, string message, params string[] fields);
    }
}