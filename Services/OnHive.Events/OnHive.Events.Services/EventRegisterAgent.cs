using OnHive.Core.Library.Contracts.Events;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Domains.Events.Models;

namespace OnHive.Events.Services
{
    /// <summary>
    /// Event Register Service
    /// </summary>
    public class EventRegisterAgent : IEventRegister
    {
        private readonly EventRegisterServiceSettings eventRegisterServiceSettings;
        private readonly IEventsService eventsService;

        public EventRegisterAgent(IEventsService service, EventRegisterServiceSettings eventRegisterServiceSettings)
        {
            this.eventRegisterServiceSettings = eventRegisterServiceSettings;
            this.eventsService = service;
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="key">Event Key</param>
        /// <param name="message">Event Message</param>
        /// <param name="fields">Event Fields: "key:value"</param>
        /// <returns></returns>
        public async Task RegisterEvent(string tenantId, string userId, string key, string message, params string[] fields)
        {
            var fieldsDictionary = fields.Select(f => new KeyValuePair<string, string>(f.Split(":")[0], f.Split(":")[-1])).ToDictionary();
            await RegisterEvent(tenantId, userId, key, message, fieldsDictionary, []);
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="key">Event Key</param>
        /// <param name="message">Event Message</param>
        /// <param name="fields">Event Field keys</param>
        /// <param name="values">Event Values ...</param>
        /// <returns></returns>
        public async Task RegisterEvent(string tenantId, string userId, string key, string message, List<string> fields, params string[] values)
        {
            await RegisterEvent(tenantId, userId, key, message, fields, values.ToList(), []);
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="key">Event Key</param>
        /// <param name="message">Event Message</param>
        /// <param name="fields">Event Field keys</param>
        /// <param name="values">Event Values</param>
        /// <param name="tags">Event Tags</param>
        /// <returns></returns>
        public async Task RegisterEvent(string tenantId, string userId, string key, string message, List<string> fields, List<string> values, List<string> tags)
        {
            var fieldsDictionary = new Dictionary<string, string>();
            var index = 0;
            foreach (var dictKey in fields)
            {
                if (index < values.Count)
                {
                    fieldsDictionary.Add(dictKey, values[index]);
                }
                else
                {
                    fieldsDictionary.Add(dictKey, string.Empty);
                }
                index++;
            }
            await RegisterEvent(tenantId, userId, key, message, fieldsDictionary, tags);
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="key">Event Key</param>
        /// <param name="message">Event Message</param>
        /// <param name="fields">Event Fields Dictionary</param>
        /// <returns></returns>
        public async Task RegisterEvent(string tenantId, string userId, string key, string message, Dictionary<string, string> fields)
        {
            await RegisterEvent(tenantId, userId, key, message, fields, []);
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="key">Event Key</param>
        /// <param name="message">Event Message</param>
        /// <param name="fields">Event Fields Dictionary</param>
        /// <param name="tags">Event Tags</param>
        /// <returns></returns>
        public async Task RegisterEvent(string tenantId, string userId, string key, string message, Dictionary<string, string> fields, List<string> tags)
        {
            var eventMessage = new EventMessage
            {
                TenantId = tenantId,
                UserId = userId,
                Key = key,
                Message = message,
                Fields = fields,
                Tags = tags,
                Origin = eventRegisterServiceSettings.Origin,
                Date = DateTime.UtcNow
            };
            foreach (var defaultField in eventRegisterServiceSettings.DefaultFields)
            {
                if (!eventMessage.Fields.ContainsKey(defaultField.Key))
                {
                    eventMessage.Fields.Add(defaultField.Key, defaultField.Value);
                }
            }
            foreach (var defaultTag in eventRegisterServiceSettings.DefaultTags)
            {
                if (!eventMessage.Tags.Contains(defaultTag))
                {
                    eventMessage.Tags.Add(defaultTag);
                }
            }
            await eventsService.ProcessEvent(eventMessage, true);
        }
    }
}