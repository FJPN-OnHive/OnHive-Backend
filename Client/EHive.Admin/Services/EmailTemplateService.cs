using EHive.Core.Library.Contracts.Emails;

namespace EHive.Admin.Services
{
    public class EmailTemplateService : ServiceBase<EmailTemplateDto>, IEmailTemplateService
    {
        public EmailTemplateService(HttpClient httpClient) : base(httpClient, "/v1/EmailTemplate")
        {
        }
    }
}