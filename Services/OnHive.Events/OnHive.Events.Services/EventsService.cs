using AutoMapper;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Validations.Common;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Events.Domain.Abstractions.Repositories;
using OnHive.Events.Domain.Models;
using Serilog;
using System.Text;
using OnHive.Core.Library.Contracts.Emails;
using MailKit.Net.Smtp;
using MimeKit;
using OnHive.Core.Library.Entities.Events;
using OnHive.Events.Domain.Abstractions.Services;

namespace OnHive.Events.Services
{
    public class EventsService : IEventsService
    {
        private readonly IEventsRepository eventsRepository;
        private readonly EventsApiSettings eventsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly IAutomationsRepository automationsRepository;
        private readonly IEventsConfigRepository eventsConfigRepository;
        private readonly HttpClient httpClient;
        private readonly ISmtpClient smtpClient;

        public EventsService(IEventsRepository eventsRepository,
                             EventsApiSettings eventsApiSettings,
                             IAutomationsRepository automationsRepository,
                             IEventsConfigRepository eventsConfigRepository,
                             IMapper mapper,
                             HttpClient httpClient,
                             ISmtpClient smtpClient)
        {
            this.eventsRepository = eventsRepository;
            this.eventsApiSettings = eventsApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            this.automationsRepository = automationsRepository;
            this.eventsConfigRepository = eventsConfigRepository;
            logger = Log.Logger;
            this.smtpClient = smtpClient;
        }

        public async Task ProcessEvent(EventMessage message, bool create)
        {
            var configuration = await GetEventConfiguration(message, create);
            if (configuration == null)
            {
                return;
            }
            var eventRegister = new EventRegister
            {
                TenantId = message.TenantId,
                Key = message.Key,
                Origin = message.Origin,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = message.UserId,
                Fields = message.Fields,
                Date = message.Date,
                IsPersistent = configuration.IsPersistent,
                Message = message.Message,
                Tags = message.Tags,
                UserId = message.UserId
            };
            await eventsRepository.SaveAsync(eventRegister);
            await ExecuteAutomations(eventRegister);
        }

        public async Task<string> HouseKeepingExecute()
        {
            var referenceDate = DateTime.UtcNow.AddDays(-eventsApiSettings.HouseKeepingDays);
            var result = await eventsRepository.RemoveNonPersistentOlderThanAsync(referenceDate);
            return $"Removed {result} old events";
        }

        public async Task<EventRegisterDto?> GetByIdAsync(string eventRegisterId)
        {
            var eventRegister = await eventsRepository.GetByIdAsync(eventRegisterId);
            return mapper.Map<EventRegisterDto>(eventRegister);
        }

        public async Task<PaginatedResult<EventRegisterDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await eventsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<EventRegisterDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<EventRegisterDto>>(result.Itens)
                };
            }
            return new PaginatedResult<EventRegisterDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<EventRegisterDto>()
            };
        }

        public async Task<PaginatedResult<EventResumeDto>> GetByFilterResumeAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await eventsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<EventResumeDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<EventResumeDto>>(result.Itens)
                };
            }
            return new PaginatedResult<EventResumeDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<EventResumeDto>()
            };
        }

        public async Task<IEnumerable<EventRegisterDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var eventRegisters = await eventsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<EventRegisterDto>>(eventRegisters);
        }

        public async Task<EventRegisterDto> SaveAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? loggedUser)
        {
            var eventRegister = mapper.Map<EventRegister>(eventRegisterDto);
            ValidatePermissions(eventRegister, loggedUser?.User);
            eventRegister.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            eventRegister.CreatedAt = DateTime.UtcNow;
            eventRegister.CreatedBy = string.IsNullOrEmpty(eventRegister.CreatedBy) ? loggedUser?.User?.Id : eventRegister.CreatedBy;

            var response = await eventsRepository.SaveAsync(eventRegister);
            return mapper.Map<EventRegisterDto>(response);
        }

        public async Task<EventRegisterDto> CreateAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? loggedUser)
        {
            if (!eventRegisterDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var eventRegister = mapper.Map<EventRegister>(eventRegisterDto);
            ValidatePermissions(eventRegister, loggedUser?.User);
            eventRegister.Id = string.Empty;
            eventRegister.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await eventsRepository.SaveAsync(eventRegister, loggedUser.User.Id);
            return mapper.Map<EventRegisterDto>(response);
        }

        public async Task<EventRegisterDto?> UpdateAsync(EventRegisterDto eventRegisterDto, LoggedUserDto? loggedUser)
        {
            if (!eventRegisterDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var eventRegister = mapper.Map<EventRegister>(eventRegisterDto);
            ValidatePermissions(eventRegister, loggedUser?.User);
            var currentEventRegister = await eventsRepository.GetByIdAsync(eventRegister.Id);
            if (currentEventRegister == null || currentEventRegister.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await eventsRepository.SaveAsync(eventRegister, loggedUser.User.Id);
            return mapper.Map<EventRegisterDto>(response);
        }

        public async Task<bool> DeleteAsync(string eventRegisterId, LoggedUserDto? loggedUser)
        {
            await eventsRepository.DeleteAsync(eventRegisterId);
            return true;
        }

        private async Task ExecuteAutomations(EventRegister eventRegister)
        {
            var automations = await automationsRepository.GetByKey(eventRegister.TenantId, eventRegister.Key);
            foreach (var automation in automations)
            {
                try
                {
                    if (!CheckConditions(automation, eventRegister))
                    {
                        continue;
                    }
                    var preparedAutomation = ReplaceFields(automation, eventRegister);
                    switch (preparedAutomation.Type)
                    {
                        case Core.Library.Enums.Events.AutomationType.WebHook:
                            await ExecuteWebHookAsync(preparedAutomation);
                            break;

                        case Core.Library.Enums.Events.AutomationType.Email:
                            await ExecuteEmailAsync(preparedAutomation);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error executing event automation for {automation.EventKey} - {automation.Id}", ex);
                }
            }
        }

        private async Task ExecuteEmailAsync(Automation automation)
        {
            var composedEmail = new ComposedEmailDto
            {
                Body = automation.Email.Body,
                From = automation.Email.From,
                SendTo = automation.Email.To.Split(';').ToList(),
                Subject = automation.Email.Subject
            };
            await SmtpSend(composedEmail);
        }

        private async Task SmtpSend(ComposedEmailDto composedEmail)
        {
            var emailMessage = new MimeMessage();
            MailboxAddress emailFrom = new MailboxAddress(composedEmail.From, "noreply@ehive.com.br");
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
            await smtpClient.ConnectAsync(eventsApiSettings.EmailService.Server, eventsApiSettings.EmailService.Port, MailKit.Security.SecureSocketOptions.Auto);
            if (eventsApiSettings.EmailService.Auth)
            {
                await smtpClient.AuthenticateAsync(eventsApiSettings.EmailService.User, eventsApiSettings.EmailService.Password);
            }
            await smtpClient.SendAsync(emailMessage);
            await smtpClient.DisconnectAsync(true);
        }

        private async Task ExecuteWebHookAsync(Automation automation)
        {
            httpClient.DefaultRequestHeaders.Clear();
            foreach (var header in automation.WebHook.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            HttpResponseMessage httpResponse;
            StringContent stringContent;
            switch (automation.WebHook.Method)
            {
                case Core.Library.Enums.Events.AutomationWebHookMethod.GET:
                    httpResponse = await httpClient.GetAsync(automation.WebHook.Url);
                    break;

                case Core.Library.Enums.Events.AutomationWebHookMethod.POST:
                    stringContent = new StringContent(automation.WebHook.Body, Encoding.UTF8, automation.WebHook.ContentType);
                    httpResponse = await httpClient.PostAsync(automation.WebHook.Url, stringContent);
                    break;

                case Core.Library.Enums.Events.AutomationWebHookMethod.PUT:
                    stringContent = new StringContent(automation.WebHook.Body, Encoding.UTF8, automation.WebHook.ContentType);
                    httpResponse = await httpClient.PutAsync(automation.WebHook.Url, stringContent);
                    break;

                case Core.Library.Enums.Events.AutomationWebHookMethod.PATCH:
                    stringContent = new StringContent(automation.WebHook.Body, Encoding.UTF8, automation.WebHook.ContentType);
                    httpResponse = await httpClient.PatchAsync(automation.WebHook.Url, stringContent);
                    break;

                default:
                    logger.Warning($"Method type not found for webHook {automation.EventKey} - {automation.Id}");
                    return;
            }
            if (httpResponse.IsSuccessStatusCode)
            {
                logger.Information($"WebHook executed successfully for {automation.EventKey} - {automation.Id} : {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");
            }
            else
            {
                logger.Warning($"WebHook executed with error for {automation.EventKey} - {automation.Id}: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");
            }
        }

        private bool CheckConditions(Automation automation, EventRegister eventRegister)
        {
            if (automation.Conditions.Count == 0)
            {
                return true;
            }
            var results = new List<bool>();
            var fieldNumericValue = 0.0;
            var conditionNumericValue = 0.0;
            DateTime fieldDataValue;
            DateTime conditionDataValue;
            foreach (var condition in automation.Conditions)
            {
                switch (condition.Type)
                {
                    case Core.Library.Enums.Events.AutomationConditionType.equal:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && eventRegister.Fields[condition.Field].Equals(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.notEqual:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && !eventRegister.Fields[condition.Field].Equals(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.greaterThan:
                        if (double.TryParse(eventRegister.Fields[condition.Field], out fieldNumericValue)
                            && double.TryParse(eventRegister.Fields[condition.Field], out conditionNumericValue))
                        {
                            results.Add(fieldNumericValue > conditionNumericValue);
                        }
                        else if (DateTime.TryParse(eventRegister.Fields[condition.Field], out fieldDataValue)
                            && DateTime.TryParse(eventRegister.Fields[condition.Field], out conditionDataValue))
                        {
                            results.Add(fieldDataValue > conditionDataValue);
                        }
                        else
                        {
                            results.Add(false);
                        }
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.greaterThanOrEqual:
                        if (double.TryParse(eventRegister.Fields[condition.Field], out fieldNumericValue)
                            && double.TryParse(eventRegister.Fields[condition.Field], out conditionNumericValue))
                        {
                            results.Add(fieldNumericValue >= conditionNumericValue);
                        }
                        else if (DateTime.TryParse(eventRegister.Fields[condition.Field], out fieldDataValue)
                            && DateTime.TryParse(eventRegister.Fields[condition.Field], out conditionDataValue))
                        {
                            results.Add(fieldDataValue >= conditionDataValue);
                        }
                        else
                        {
                            results.Add(false);
                        }
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.lessThan:
                        if (double.TryParse(eventRegister.Fields[condition.Field], out fieldNumericValue)
                          && double.TryParse(eventRegister.Fields[condition.Field], out conditionNumericValue))
                        {
                            results.Add(fieldNumericValue < conditionNumericValue);
                        }
                        else if (DateTime.TryParse(eventRegister.Fields[condition.Field], out fieldDataValue)
                            && DateTime.TryParse(eventRegister.Fields[condition.Field], out conditionDataValue))
                        {
                            results.Add(fieldDataValue < conditionDataValue);
                        }
                        else
                        {
                            results.Add(false);
                        }
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.lessThanOrEqual:
                        if (double.TryParse(eventRegister.Fields[condition.Field], out fieldNumericValue)
                          && double.TryParse(eventRegister.Fields[condition.Field], out conditionNumericValue))
                        {
                            results.Add(fieldNumericValue <= conditionNumericValue);
                        }
                        else if (DateTime.TryParse(eventRegister.Fields[condition.Field], out fieldDataValue)
                            && DateTime.TryParse(eventRegister.Fields[condition.Field], out conditionDataValue))
                        {
                            results.Add(fieldDataValue <= conditionDataValue);
                        }
                        else
                        {
                            results.Add(false);
                        }
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.contains:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && eventRegister.Fields[condition.Field].Contains(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.notContains:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && !eventRegister.Fields[condition.Field].Contains(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.startsWith:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && eventRegister.Fields[condition.Field].StartsWith(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.endsWith:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && eventRegister.Fields[condition.Field].EndsWith(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.notStartsWith:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && !eventRegister.Fields[condition.Field].StartsWith(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.notEndsWith:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                            && !eventRegister.Fields[condition.Field].EndsWith(condition.Condition, StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.isNull:
                        results.Add(!eventRegister.Fields.ContainsKey(condition.Field)
                            || string.IsNullOrEmpty(eventRegister.Fields[condition.Field]));
                        break;

                    case Core.Library.Enums.Events.AutomationConditionType.isNotNull:
                        results.Add(eventRegister.Fields.ContainsKey(condition.Field)
                           && !string.IsNullOrEmpty(eventRegister.Fields[condition.Field]));
                        break;

                    default:
                        break;
                }
            }
            return results.TrueForAll(r => r);
        }

        private Automation ReplaceFields(Automation automation, EventRegister eventRegister)
        {
            foreach (var field in eventRegister.Fields)
            {
                if (automation.Email != null)
                {
                    automation.Email.Body = automation.Email.Body.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.Email.Subject = automation.Email.Subject.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.Email.To = automation.Email.To.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.Email.From = automation.Email.From.Replace($"{{{{{field.Key}}}}}", field.Value);
                }
                if (automation.WebHook != null)
                {
                    automation.WebHook.Body = automation.WebHook.Body.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.WebHook.Url = automation.WebHook.Url.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.WebHook.ContentType = automation.WebHook.ContentType.Replace($"{{{{{field.Key}}}}}", field.Value);
                    automation.WebHook.Headers = automation.WebHook.Headers.Select(h =>
                    {
                        return new KeyValuePair<string, string>
                        (
                            h.Key.Replace($"{{{{{field.Key}}}}}", field.Value),
                            h.Value.Replace($"{{{{{field.Key}}}}}", field.Value)
                        );
                    }).ToDictionary();
                }
            }
            return automation;
        }

        private async Task<EventConfig> GetEventConfiguration(EventMessage message, bool create)
        {
            var eventConfig = await eventsConfigRepository.GetByKeyAndOrigin(message.TenantId, message.Key, message.Origin);
            if (eventConfig == null && create)
            {
                eventConfig = await eventsConfigRepository.GetByKeyAndOrigin(string.Empty, message.Key, message.Origin);
                if (eventConfig == null)
                {
                    eventConfig = new EventConfig
                    {
                        TenantId = string.Empty,
                        Key = message.Key,
                        Origin = message.Origin,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        Fields = message.Fields
                            .Select(f => new EventConfigFields { Key = f.Key, Description = f.Key })
                            .ToList()
                    };
                }
                else
                {
                    eventConfig.TenantId = message.TenantId;
                    eventConfig.Id = string.Empty;
                }
                await eventsConfigRepository.SaveAsync(eventConfig);
            }
            return eventConfig;
        }

        private void ValidatePermissions(EventRegister eventRegister, UserDto? loggedUser)
        {
            if (loggedUser != null && eventRegister.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID EventRegister/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    eventRegister.Id, eventRegister.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}