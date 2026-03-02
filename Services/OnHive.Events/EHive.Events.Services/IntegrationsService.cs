using AutoMapper;
using EHive.Core.Library.Contracts.Events;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Entities.Events;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Extensions;
using EHive.Events.Domain.Abstractions.Repositories;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Events.Domain.Models;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json;

namespace EHive.Events.Services
{
    public class IntegrationsService : IIntegrationsService
    {
        private readonly IMauticIntegrationRepository mauticIntegrationRepository;
        private readonly HttpClient httpClient;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public IntegrationsService(IMauticIntegrationRepository mauticIntegrationRepository, HttpClient httpClient, IMapper mapper)
        {
            this.mauticIntegrationRepository = mauticIntegrationRepository;
            this.httpClient = httpClient;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task MauticAPI(string tenantId, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var integration = await GetMauticIntegration(tenantId, headers, query);
            StartHttpClient(integration);
            await UpdateFieldsAsync(integration);
            var data = GetIntegrationDataFromQuery(query);
            await SendDataAsync(integration, data);
        }

        public async Task MauticAPI(string tenantId, JsonDocument body, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var integration = await GetMauticIntegration(tenantId, headers, query);
            StartHttpClient(integration);
            await UpdateFieldsAsync(integration);
            var data = GetIntegrationDataFromBody(body);
            await SendDataAsync(integration, data);
        }

        public async Task MauticAPI(string tenantId, Dictionary<string, StringValues> formData, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var integration = await GetMauticIntegration(tenantId, headers, query);
            StartHttpClient(integration);
            await UpdateFieldsAsync(integration);
            var data = GetIntegrationDataFromFormData(formData);
            await SendDataAsync(integration, data);
        }

        public async Task<MauticIntegrationDto> GetMauticSettings(LoggedUserDto loggedUser)
        {
            var integration = await mauticIntegrationRepository.GetMauticIntegrationByTenantId(loggedUser.User.TenantId, false)
              ?? throw new NotFoundException("Mautic integration not found for tenant.");
            return mapper.Map<MauticIntegrationDto>(integration);
        }

        public async Task<MauticIntegrationDto> CreateMauticSettings(MauticIntegrationDto mauticIntegration, LoggedUserDto loggedUser)
        {
            var integration = mapper.Map<MauticIntegration>(mauticIntegration);
            var currentIntegration = await mauticIntegrationRepository.GetMauticIntegrationByTenantId(loggedUser.User.TenantId, false);
            if (currentIntegration != null)
            {
                integration.Id = currentIntegration.Id;
            }
            integration.TenantId = loggedUser.User.TenantId;
            await mauticIntegrationRepository.SaveAsync(integration, loggedUser.User.Id);
            return mapper.Map<MauticIntegrationDto>(integration);
        }

        public async Task<MauticIntegrationDto> UpdateMauticSettings(MauticIntegrationDto mauticIntegration, LoggedUserDto loggedUser)
        {
            var integration = mapper.Map<MauticIntegration>(mauticIntegration);
            var currentIntegration = await mauticIntegrationRepository.GetMauticIntegrationByTenantId(loggedUser.User.TenantId, false)
                ?? throw new NotFoundException("Mautic integration not found for tenant.");
            integration.Id = currentIntegration.Id;
            integration.TenantId = loggedUser.User.TenantId;
            await mauticIntegrationRepository.SaveAsync(integration, loggedUser.User.Id);
            return mapper.Map<MauticIntegrationDto>(integration);
        }

        private async Task<MauticIntegration> GetMauticIntegration(string tenantId, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
            var integration = await mauticIntegrationRepository.GetMauticIntegrationByTenantId(tenantId, true)
                ?? throw new NotFoundException("Mautic integration not found for tenant.");
            var apiKey = query.ContainsKey("X-API-Key")
                ? query["X-API-Key"]
                : headers.ContainsKey("X-API-Key")
                ? headers["X-API-Key"]
                : string.Empty;
            if (apiKey != integration.IntegrationAPIKey)
            {
                logger.Error("Invalid API Key for Mautic integration for tenant {0}.", tenantId);
                throw new UnauthorizedAccessException("Invalid API Key.");
            }
            return integration;
        }

        private async Task SendDataAsync(MauticIntegration integration, IntegrationData data)
        {
            var contact = GetMauticContact(data);
            var response = await httpClient.PostAsJsonAsync("api/contacts/new", contact);
            if (!response.IsSuccessStatusCode)
            {
                logger.Error("Failed to create contact in Mautic for tenant {0}. Status code: {1}, Reason: {2}", integration.TenantId, response.StatusCode, response.ReasonPhrase);
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create contact in Mautic: {response.ReasonPhrase} - {responseContent}");
            }
        }

        private IntegrationData GetIntegrationDataFromFormData(Dictionary<string, StringValues> formData)
        {
            var data = new IntegrationData
            {
                Name = formData.ContainsKey("name") ? formData["name"]! : string.Empty,
                Email = formData.ContainsKey("email") ? formData["email"]! : string.Empty,
                Phone = formData.ContainsKey("phone") ? formData["phone"]! : string.Empty,
                Tag = formData.ContainsKey("tag") ? formData["tag"]! : string.Empty,
                Conversion = formData.ContainsKey("conversion") ? formData["conversion"]! : string.Empty,
                FormId = formData.ContainsKey("formId") ? formData["formId"]! : string.Empty,
                FormName = formData.ContainsKey("formName") ? formData["formName"]! : string.Empty,
                Acceptance = formData.ContainsKey("acceptance") &&
                (formData["acceptance"]!.ToString().Equals("true", StringComparison.OrdinalIgnoreCase)
                || formData["acceptance"]!.ToString().Equals("on", StringComparison.OrdinalIgnoreCase)
                || formData["acceptance"]!.ToString().Equals("yes", StringComparison.OrdinalIgnoreCase))
            };
            return data;
        }

        private IntegrationData GetIntegrationDataFromQuery(Dictionary<string, string> query)
        {
            var data = new IntegrationData
            {
                Name = query.ContainsKey("name") ? query["name"] : string.Empty,
                Email = query.ContainsKey("email") ? query["email"] : string.Empty,
                Phone = query.ContainsKey("phone") ? query["phone"] : string.Empty,
                Tag = query.ContainsKey("tag") ? query["tag"] : string.Empty,
                Conversion = query.ContainsKey("conversion") ? query["conversion"] : string.Empty,
                FormId = query.ContainsKey("formId") ? query["formId"] : string.Empty,
                FormName = query.ContainsKey("formName") ? query["formName"] : string.Empty,
                Acceptance = query.ContainsKey("acceptance") &&
                (query["acceptance"].Equals("true", StringComparison.OrdinalIgnoreCase)
                || query["acceptance"].Equals("on", StringComparison.OrdinalIgnoreCase)
                || query["acceptance"].Equals("yes", StringComparison.OrdinalIgnoreCase))
            };
            return data;
        }

        private IntegrationData GetIntegrationDataFromBody(JsonDocument body)
        {
            var acceptance = "false";
            if (body.RootElement.TryGetProperty("acceptance", out var acceptanceString))
            {
                acceptance = acceptanceString.GetString() ?? "false";
            }

            var data = new IntegrationData
            {
                Name = body.RootElement.GetProperty("name").GetString() ?? string.Empty,
                Email = body.RootElement.GetProperty("email").GetString() ?? string.Empty,
                Phone = body.RootElement.GetProperty("phone").GetString() ?? string.Empty,
                Tag = body.RootElement.GetProperty("tag").GetString() ?? string.Empty,
                Conversion = body.RootElement.GetProperty("conversion").GetString() ?? string.Empty,
                FormId = body.RootElement.GetProperty("formId").GetString() ?? string.Empty,
                FormName = body.RootElement.GetProperty("formName").GetString() ?? string.Empty,
                Acceptance = acceptance.Equals("true", StringComparison.OrdinalIgnoreCase)
                                || acceptance.Equals("on", StringComparison.OrdinalIgnoreCase)
                                || acceptance.Equals("yes", StringComparison.OrdinalIgnoreCase)
            };
            return data;
        }

        private async Task UpdateFieldsAsync(MauticIntegration integration)
        {
            httpClient.BaseAddress = new Uri(integration.MauticUrl);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", AuthorizeMautic(integration));
            var response = await httpClient.GetAsync("api/fields/contact");
            if (!response.IsSuccessStatusCode)
            {
                logger.Error("Failed to get fields contact in Mautic for tenant {0}. Status code: {1}, Reason: {2}", integration.TenantId, response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException($"Failed to get fields for contacts in Mautic: {response.ReasonPhrase}");
            }
            var fieldsResponse = await response.Content.ReadAsStringAsync();
            var fieldsJson = JsonDocument.Parse(fieldsResponse);
            var fields = fieldsJson.RootElement.GetProperty("fields").EnumerateObject().Select(j => j.Value.GetProperty("alias").GetString());
            if (!fields.Any(f => f.Equals("conversion", StringComparison.InvariantCultureIgnoreCase)))
            {
                var field = new MauticField
                {
                    Alias = "conversion",
                    Label = "Conversion",
                    Type = "text",
                    Group = "core",
                    Object = "lead",
                    IsPublished = true
                };
                await AddFieldAsync(integration, field);
            }

            if (!fields.Any(f => f.Equals("formid", StringComparison.InvariantCultureIgnoreCase)))
            {
                var field = new MauticField
                {
                    Alias = "formid",
                    Label = "Form Id",
                    Type = "text",
                    Group = "core",
                    Object = "lead",
                    IsPublished = true
                };
                await AddFieldAsync(integration, field);
            }

            if (!fields.Any(f => f.Equals("formname", StringComparison.InvariantCultureIgnoreCase)))
            {
                var field = new MauticField
                {
                    Alias = "formname",
                    Label = "Form Name",
                    Type = "text",
                    Group = "core",
                    Object = "lead",
                    IsPublished = true
                };
                await AddFieldAsync(integration, field);
            }

            if (!fields.Any(f => f.Equals("termos_de_aceite", StringComparison.InvariantCultureIgnoreCase)))
            {
                var field = new MauticField
                {
                    Alias = "termos_de_aceite",
                    Label = "Termos de aceite",
                    Type = "select",
                    Group = "core",
                    DefaultValue = "nulo",
                    Object = "lead",
                    IsPublished = true,
                    Properties = new MauticFieldProperty
                    {
                        List = new List<MauticProperty>
                        {
                            new MauticProperty { Label = "Consentimento", Value = "consentimento" },
                            new MauticProperty { Label = "Recusado", Value = "recusado" },
                            new MauticProperty { Label = "Legítimo interesse", Value = "legitimo_interesse" },
                            new MauticProperty { Label = "Nulo", Value = "nulo" }
                        }
                    }
                };
                await AddFieldAsync(integration, field);
            }
        }

        private async Task AddFieldAsync(MauticIntegration integration, MauticField field)
        {
            var response = await httpClient.PostAsJsonAsync("api/fields/contact/new", field);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                logger.Error("Failed to get fields contact in Mautic for tenant {0}. Status code: {1}, Reason: {2} text: {3}", integration.TenantId, response.StatusCode, response.ReasonPhrase, responseContent);
                throw new HttpRequestException($"Failed to get fields for contacts in Mautic: {response.ReasonPhrase}");
            }
        }

        private static string AuthorizeMautic(MauticIntegration integration)
        {
            var password = $"{integration.MauticClientId}:{integration.MauticClientSecret}";
            var encodedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            return encodedPassword;
        }

        private static MauticContact GetMauticContact(IntegrationData data)
        {
            var result = new MauticContact
            {
                Firstname = data.Name,
                Email = data.Email,
                Mobile = data.Phone.RemoveMask(),
                Conversion = data.Conversion,
                FormId = data.FormId,
                FormName = data.FormName,
                TermosDeAceite = data.Acceptance ? "consentimento" : "recusado",
                Tags = [
                    "ONHIVE:INTEGRATION"
                ]
            };
            if (!string.IsNullOrWhiteSpace(data.Tag))
            {
                result.Tags.AddRange(data.Tag.Split(','));
            }
            if (!string.IsNullOrWhiteSpace(result.FormId))
            {
                result.Tags.Add($"FORM_ID:{data.FormId}");
            }
            if (!string.IsNullOrWhiteSpace(result.FormName))
            {
                result.Tags.Add($"FORM_NAME:{data.FormName}");
            }
            if (!string.IsNullOrWhiteSpace(result.Conversion))
            {
                result.Tags.Add($"CONVERSION:{data.Conversion}");
            }
            if (data.Acceptance)
            {
                result.Tags.Add("ACCEPTANCE:TRUE");
            }
            else
            {
                result.Tags.Add("ACCEPTANCE:FALSE");
            }

            return result;
        }

        private void StartHttpClient(MauticIntegration integration)
        {
            httpClient.BaseAddress = new Uri(integration.MauticUrl);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", AuthorizeMautic(integration));
        }
    }
}