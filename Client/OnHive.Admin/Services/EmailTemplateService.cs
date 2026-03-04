using OnHive.Core.Library.Contracts.Emails;

namespace OnHive.Admin.Services
{
    public class EmailTemplateService : ServiceBase<EmailTemplateDto>, IEmailTemplateService
    {
        public EmailTemplateService(HttpClient httpClient) : base(httpClient, "/v1/EmailTemplate")
        {
        }
    }
}