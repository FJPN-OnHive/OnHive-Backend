using OnHive.Core.Library.Attributes;

namespace OnHive.Events.Domain.Models
{
    public class EventsApiSettings
    {
        public string? EventsAdminPermission { get; set; } = "events_admin";

        public int HouseKeepingDays { get; set; } = 7;

        public List<string> RestrictedFields { get; set; } = ["Id", "TenantId", "Slug", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "Version"];

        public List<string> RestrictedCollections { get; set; } = ["Apidocs", "Tenants", "TenantParameters", "SystemParameters", "Users", "Roles", "ProductCourse", "Events", "Messages", "Search", "SystemFeatures", "WebHooks"];

        [ValidateObject]
        public EmailService EmailService { get; set; } = new();

        public List<Cronjob> Cronjobs { get; set; } = [];
    }

    public class EmailService
    {
        public string Server { get; set; } = string.Empty;

        public int Port { get; set; } = 0;

        public bool Auth { get; set; } = true;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}