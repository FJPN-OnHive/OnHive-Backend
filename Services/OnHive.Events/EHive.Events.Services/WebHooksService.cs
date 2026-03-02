using AutoMapper;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Events;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Events.Domain.Abstractions.Repositories;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Events.Domain.Models;
using Serilog;
using System.Text.Json;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Enums.WebHook;
using MoonSharp.Interpreter;
using EHive.Users.Domain.Abstractions.Services;

namespace EHive.Events.Services
{
    public class WebHooksService : IWebHooksService
    {
        private readonly IWebHooksRepository webHooksRepository;
        private readonly EventsApiSettings eventsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly ILoginService loginService;

        public WebHooksService(IWebHooksRepository webHooksRepository, EventsApiSettings eventsApiSettings, IMapper mapper, ILoginService loginService)
        {
            this.webHooksRepository = webHooksRepository;
            this.eventsApiSettings = eventsApiSettings;
            this.mapper = mapper;
            this.loginService = loginService;
            logger = Log.Logger;
        }

        public async Task Receive(string tenantId, string slug, string method, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query, bool authorized)
        {
            var apiKey = headers.GetValueOrDefault("x-api-key");
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = query.GetValueOrDefault("apiKey");
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new UnauthorizedAccessException("API Key not found");
                }
            }
            var webhook = await webHooksRepository.GetBySlug(tenantId, slug) ?? throw new NotFoundException("WebHook not found");
            if (!webhook.UseAuthorization && !apiKey.Equals(webhook.ApiKey, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid API Key");
            }
            else if (!authorized && webhook.UseAuthorization)
            {
                throw new UnauthorizedAccessException("Invalid Authorization");
            }
            if (!method.Equals(webhook.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("Method not allowed");
            }
            await ProcessWebHook(tenantId, webhook, body, headers, query);
        }

        public async Task<WebHookDto?> GetByIdAsync(string webHookId)
        {
            var webHook = await webHooksRepository.GetByIdAsync(webHookId);
            return mapper.Map<WebHookDto>(webHook);
        }

        public async Task<PaginatedResult<WebHookDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await webHooksRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<WebHookDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<WebHookDto>>(result.Itens)
                };
            }
            return new PaginatedResult<WebHookDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<WebHookDto>()
            };
        }

        public async Task<IEnumerable<WebHookDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var webHooks = await webHooksRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<WebHookDto>>(webHooks);
        }

        public async Task<WebHookDto> SaveAsync(WebHookDto webHookDto, LoggedUserDto? loggedUser)
        {
            var webHook = mapper.Map<WebHook>(webHookDto);
            ValidatePermissions(webHook, loggedUser?.User);
            webHook.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            webHook.CreatedAt = DateTime.UtcNow;
            webHook.IsActive = true;
            webHook.CreatedBy = string.IsNullOrEmpty(webHook.CreatedBy) ? loggedUser?.User?.Id : webHook.CreatedBy;
            await ValidateDuplicateAsync(webHook, loggedUser?.User);
            var response = await webHooksRepository.SaveAsync(webHook);
            return mapper.Map<WebHookDto>(response);
        }

        public async Task<WebHookDto> CreateAsync(WebHookDto webHookDto, LoggedUserDto? loggedUser)
        {
            if (!webHookDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var webHook = mapper.Map<WebHook>(webHookDto);
            ValidatePermissions(webHook, loggedUser?.User);
            webHook.Id = string.Empty;
            webHook.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            webHook.IsActive = true;
            await ValidateDuplicateAsync(webHook, loggedUser?.User);
            var response = await webHooksRepository.SaveAsync(webHook, loggedUser.User.Id);
            return mapper.Map<WebHookDto>(response);
        }

        public async Task<WebHookDto?> UpdateAsync(WebHookDto webHookDto, LoggedUserDto? loggedUser)
        {
            if (!webHookDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var webHook = mapper.Map<WebHook>(webHookDto);
            ValidatePermissions(webHook, loggedUser?.User);
            var currentWebHook = await webHooksRepository.GetByIdAsync(webHook.Id);
            if (currentWebHook == null || currentWebHook.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            await ValidateDuplicateAsync(webHook, loggedUser?.User);
            var response = await webHooksRepository.SaveAsync(webHook, loggedUser.User.Id);
            return mapper.Map<WebHookDto>(response);
        }

        public async Task DeleteById(string webHookId, LoggedUserDto? loggedUser)
        {
            var currentWebHook = await webHooksRepository.GetByIdAsync(webHookId);
            ValidatePermissions(currentWebHook, loggedUser?.User);
            await webHooksRepository.DeleteAsync(webHookId);
        }

        private async Task ProcessWebHook(string tenantId, WebHook webhook, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var impersonated = await GetImpersonatedAsync(webhook.UserId);
            var variables = new Dictionary<string, string>();
            foreach (var step in webhook.Steps)
            {
                switch (step.Type)
                {
                    case WebHookStepTypes.Action:
                        await ExecuteActions(tenantId, step.Actions, body, headers, query);
                        break;

                    case WebHookStepTypes.Python:
                        if (impersonated == null)
                        {
                            throw new UnauthorizedAccessException("Impersonated user not found");
                        }
                        variables = await ExecutePythonScript(impersonated, step.Script, body, headers, query, variables);
                        break;

                    case WebHookStepTypes.Lua:
                        if (impersonated == null)
                        {
                            throw new UnauthorizedAccessException("Impersonated user not found");
                        }
                        variables = ExecuteLuaScript(impersonated, step.Script, body, headers, query, variables);
                        break;

                    case WebHookStepTypes.JavaScript:
                        if (impersonated == null)
                        {
                            throw new UnauthorizedAccessException("Impersonated user not found");
                        }
                        variables = await ExecuteJavaScript(impersonated, step.Script, body, headers, query, variables);
                        break;

                    default:
                        break;
                }
            }
        }

        private async Task<LoggedUserDto> GetImpersonatedAsync(string userID)
        {
            if (!string.IsNullOrEmpty(userID))
            {
                var user = await loginService.ImpersonateAsync(userID);
                if (user != null)
                {
                    return new LoggedUserDto(user.User, user.Token);
                }
                else
                {
                    throw new UnauthorizedAccessException($"User not found: {userID}");
                }
            }
            else
            {
                return null;
            }
        }

        private async Task ExecuteActions(string tenantId, List<WebHookAction> actions, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            foreach (var action in actions)
            {
                if (eventsApiSettings.RestrictedCollections.Exists(c => action.TargetCollection.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new UnauthorizedAccessException("Restricted collection");
                }
                if (eventsApiSettings.RestrictedFields.Exists(f => action.TargetField.Equals(f, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new UnauthorizedAccessException("Restricted field");
                }
                await webHooksRepository.ExecuteAction(tenantId, action, body, headers, query);
            }
        }

        private Dictionary<string, string> ExecuteLuaScript(LoggedUserDto loggedUser, string? script, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query, Dictionary<string, string> variables)
        {
            try
            {
                Script scriptEngine = new Script();
                scriptEngine.Globals["body"] = body;
                scriptEngine.Globals["headers"] = headers;
                scriptEngine.Globals["query"] = query;
                scriptEngine.Globals["token"] = loggedUser.Token;
                scriptEngine.Globals["variables"] = variables;
                DynValue result = scriptEngine.DoString(script);

                if (result.Type == DataType.Table)
                {
                    var table = result.Table;
                    foreach (var pair in table.Pairs)
                    {
                        if (variables.ContainsKey(pair.Key.CastToString()))
                        {
                            variables[pair.Key.CastToString()] = pair.Value.CastToString();
                        }
                        else
                        {
                            variables.Add(pair.Key.CastToString(), pair.Value.CastToString());
                        }
                    }
                }
                return variables;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid script", ex);
            }
        }

        private async Task<Dictionary<string, string>> ExecutePythonScript(LoggedUserDto loggedUser, string? script, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query, Dictionary<string, string> variables)
        {
            throw new NotImplementedException();
        }

        private async Task<Dictionary<string, string>> ExecuteJavaScript(LoggedUserDto loggedUser, string? script, JsonDocument? body, Dictionary<string, string> headers, Dictionary<string, string> query, Dictionary<string, string> variables)
        {
            try
            {
                var engine = new Jint.Engine();
                engine.SetValue("body", body);
                engine.SetValue("headers", headers);
                engine.SetValue("query", query);
                engine.SetValue("token", loggedUser.Token);
                engine.SetValue("variables", variables);
                engine.SetValue("log", new Action<object>(Console.WriteLine));
                engine.Execute(script);
                variables = (Dictionary<string, string>)engine.GetValue("variables").ToObject();
                return variables;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid script", ex);
            }
        }

        private async Task ValidateDuplicateAsync(WebHook webHook, UserDto? loggedUser)
        {
            if (webHook.Id == null)
            {
                var existingWebHook = await webHooksRepository.GetBySlug(webHook.TenantId, webHook.Slug);
                if (existingWebHook != null && existingWebHook.Id != webHook.Id)
                {
                    logger.Warning("WebHook already exists: {slug}", webHook.Slug);
                    throw new DuplicatedException("WebHook already exists");
                }
            }
        }

        private void ValidatePermissions(WebHook webHook, UserDto? loggedUser)
        {
            if (loggedUser != null && webHook.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID WebHook/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    webHook.Id, webHook.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}