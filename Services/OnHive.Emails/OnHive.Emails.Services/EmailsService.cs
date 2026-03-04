using AutoMapper;
using OnHive.Configuration.Library.Exceptions;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Emails;
using OnHive.Core.Library.Helpers;
using OnHive.Emails.Domain.Abstractions.Repositories;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Emails.Domain.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnHive.Emails.Services
{
    public class EmailsService : IEmailsService
    {
        private const string defaultTenantId = "00000000000000";

        private readonly IEmailsRepository emailsRepository;
        private readonly EmailsApiSettings emailsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public EmailsService(IEmailsRepository emailsRepository, EmailsApiSettings emailsApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.emailsRepository = emailsRepository;
            this.emailsApiSettings = emailsApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<EmailTemplateDto?> GetByIdAsync(string emailTemplateId)
        {
            var emailTemplate = await emailsRepository.GetByIdAsync(emailTemplateId);
            return mapper.Map<EmailTemplateDto>(emailTemplate);
        }

        public async Task<PaginatedResult<EmailTemplateDto>> GetByFilterAsync(RequestFilter filter, UserDto? loggedUser)
        {
            var result = await emailsRepository.GetByFilterAsync(filter, loggedUser?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<EmailTemplateDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<EmailTemplateDto>>(result.Itens)
                };
            }
            return new PaginatedResult<EmailTemplateDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<EmailTemplateDto>()
            };
        }

        public async Task<IEnumerable<EmailTemplateDto>> GetAllAsync(UserDto? loggedUser)
        {
            var emailTemplates = await emailsRepository.GetAllAsync(loggedUser?.TenantId);
            return mapper.Map<IEnumerable<EmailTemplateDto>>(emailTemplates);
        }

        public async Task<EmailTemplateDto> SaveAsync(EmailTemplateDto emailTemplateDto, UserDto? loggedUser)
        {
            var emailTemplate = mapper.Map<EmailTemplate>(emailTemplateDto);
            ValidatePermissions(emailTemplate, loggedUser);
            emailTemplate.TenantId = loggedUser.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            emailTemplate.CreatedAt = DateTime.UtcNow;
            emailTemplate.CreatedBy = string.IsNullOrEmpty(emailTemplate.CreatedBy) ? loggedUser.Id : emailTemplate.CreatedBy;

            var response = await emailsRepository.SaveAsync(emailTemplate);
            return mapper.Map<EmailTemplateDto>(response);
        }

        public async Task<EmailTemplateDto> CreateAsync(EmailTemplateDto emailTemplateDto, UserDto loggedUser)
        {
            var emailTemplate = mapper.Map<EmailTemplate>(emailTemplateDto);
            ValidatePermissions(emailTemplate, loggedUser);
            emailTemplate.Id = string.Empty;
            emailTemplate.TenantId = loggedUser.TenantId;
            var response = await emailsRepository.SaveAsync(emailTemplate, loggedUser.Id);
            return mapper.Map<EmailTemplateDto>(response);
        }

        public async Task<EmailTemplateDto?> UpdateAsync(EmailTemplateDto emailTemplateDto, UserDto loggedUser)
        {
            var emailTemplate = mapper.Map<EmailTemplate>(emailTemplateDto);
            ValidatePermissions(emailTemplate, loggedUser);
            var currentEmailTemplate = await emailsRepository.GetByIdAsync(emailTemplate.Id);
            if (currentEmailTemplate == null || currentEmailTemplate.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            var response = await emailsRepository.SaveAsync(emailTemplate, loggedUser.Id);
            return mapper.Map<EmailTemplateDto>(response);
        }

        public async Task<EmailTemplateDto?> PatchAsync(JsonDocument patch, UserDto loggedUser)
        {
            var currentEmailTemplate = await emailsRepository.GetByIdAsync(patch.GetId());
            if (currentEmailTemplate == null || currentEmailTemplate.TenantId != loggedUser.TenantId)
            {
                return null;
            }
            currentEmailTemplate = patch.PatchEntity(currentEmailTemplate);
            ValidatePermissions(currentEmailTemplate, loggedUser);
            var response = await emailsRepository.SaveAsync(currentEmailTemplate, loggedUser.Id);
            return mapper.Map<EmailTemplateDto>(response);
        }

        public async Task<bool> DeleteAsync(string id, UserDto loggedUser)
        {
            var currentEmailTemplate = await emailsRepository.GetByIdAsync(id);
            if (currentEmailTemplate == null || currentEmailTemplate.TenantId != loggedUser.TenantId)
            {
                return false;
            }
            ValidatePermissions(currentEmailTemplate, loggedUser);
            await emailsRepository.DeleteAsync(id);
            return true;
        }

        public async Task ComposeEmail(EmailSendDto message)
        {
            var template = await GetTemplate(message);
            var service = GetService(message);
            var composedEmail = CreateComposedEmail(message, template);
            try
            {
                await SendToService(service, composedEmail);
            }
            catch (Exception ex)
            {
                logger.Error("Error sending email for service {service}: {message}", service.Key, ex.Message, ex);
                throw;
            }
        }

        public async Task Migrate()
        {
            var file = GetMigrationFile();
            var current = await emailsRepository.GetAllAsync(defaultTenantId);
            if (!current.Any())
            {
                foreach (var template in file)
                {
                    template.TenantId = defaultTenantId;
                    await emailsRepository.SaveAsync(template);
                }
            }
        }

        private void SmtpSend(EmailService service, ComposedEmailDto composedEmail)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                MailboxAddress emailFrom = new MailboxAddress(service.From, service.From);
                emailMessage.From.Add(emailFrom);
                foreach (var sendTo in composedEmail.SendTo)
                {
                    MailboxAddress emailTo = new MailboxAddress(sendTo, sendTo);
                    emailMessage.To.Add(emailTo);
                }
                emailMessage.Subject = composedEmail.Subject;
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = composedEmail.Body;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                using (SmtpClient mailClient = new SmtpClient())
                {
                    mailClient.Connect(service.Server, service.Port, MailKit.Security.SecureSocketOptions.Auto);
                    if (service.Auth)
                    {
                        mailClient.RequireTLS = true;
                        mailClient.Authenticate(service.User, service.Password);
                    }
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }
            }
        }

        private EmailService GetService(EmailSendDto message)
        {
            var service = emailsApiSettings.EmailServices.FirstOrDefault(s => s.Key.Equals(message.ServiceCode));
            service ??= emailsApiSettings.EmailServices.FirstOrDefault(s => s.IsDefault)
                    ?? throw new ArgumentException($"Invalid service {message.TemplateCode}");
            return service;
        }

        private async Task<EmailTemplate> GetTemplate(EmailSendDto message)
        {
            var template = await emailsRepository.GetByCodeAsync(message.TemplateCode, message.TenantId);
            if (template == null)
            {
                await MigrateToTenant(message.TenantId);
                template = await emailsRepository.GetByCodeAsync(message.TemplateCode, message.TenantId);
            }
            return template ?? throw new ArgumentException($"Invalid template {message.TemplateCode}");
        }

        private async Task MigrateToTenant(string tenantId)
        {
            var templates = await emailsRepository.GetAllAsync(tenantId);
            var defaultTemplates = await emailsRepository.GetAllAsync(defaultTenantId);

            foreach (var defaultTemplate in defaultTemplates)
            {
                var template = templates.Find(t => t.Code != defaultTemplate.Code);
                if (template == null)
                {
                    defaultTemplate.TenantId = tenantId;
                    await emailsRepository.SaveAsync(defaultTemplate);
                }
            }
        }

        private async Task SendToService(EmailService service, ComposedEmailDto composedEmail)
        {
            if (!service.IsMock)
            {
                SmtpSend(service, composedEmail);
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(composedEmail), new MediaTypeHeaderValue("application/json"));
                var response = await httpClient.PostAsync($"{service.Server}:{service.Port}/v1/internal/emailSend", content);
                response.EnsureSuccessStatusCode();
            }
        }

        private ComposedEmailDto CreateComposedEmail(EmailSendDto message, EmailTemplate template)
        {
            if (!message.Fields.ContainsKey("email"))
            {
                message.Fields.Add("email", message.From);
            }
            return new ComposedEmailDto
            {
                TenantId = message.TenantId,
                Attachments = message.Attachments,
                From = message.From,
                SendTo = message.SendTo,
                Account = message.AccountCode,
                Body = ReplaceTags(template.Body, message.Fields),
                Subject = ReplaceTags(template.Subject, message.Fields),
                SendDate = DateTime.UtcNow
            };
        }

        private string ReplaceTags(string input, Dictionary<string, string> fields)
        {
            var result = input;
            foreach (var field in fields)
            {
                result = result.Replace($"&{field.Key};", field.Value);
            }
            return result;
        }

        private void ValidatePermissions(EmailTemplate emailTemplate, UserDto? loggedUser)
        {
            if (loggedUser != null && emailTemplate.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID EmailTemplate/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    emailTemplate.Id, emailTemplate.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private List<EmailTemplate> GetMigrationFile()
        {
            var file = JsonSerializer.Deserialize<List<EmailTemplate>>(File.ReadAllText(Path.Join("Migrations", "defaultEmailTemplates.json")));
            if (file == null)
            {
                logger.Error("Missing features migration file.");
                throw new MissingConfigurationException<List<EmailTemplate>>("defaultEmailTemplates.json");
            }
            return file;
        }
    }
}