using System.ComponentModel.DataAnnotations;

namespace EHive.Emails.Domain.Models
{
    public class EmailsApiSettings
    {
        public string? EmailsAdminPermission { get; set; } = "emails_admin";

        public List<EmailService> EmailServices { get; set; } = new();
    }

    public class EmailService
    {
        public string Key { get; set; } = string.Empty;

        public string Server { get; set; } = string.Empty;

        public int Port { get; set; } = 0;

        public bool Auth { get; set; } = true;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool IsMock { get; set; } = false;

        public bool IsDefault { get; set; } = false;

        public string? From { get; set; }

        public string? FromName { get; set; }
    }
}