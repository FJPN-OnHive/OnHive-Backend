using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Events;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class MauticIntegrationService : IMauticIntegrationService
    {
        private readonly HttpClient httpClient;

        public MauticIntegrationService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<MauticIntegrationDto?> GetIntegrationSettings(string tenantId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"/v1/Integrations/MauticSettings");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<Response<MauticIntegrationDto>>(await response.Content.ReadAsStringAsync());
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new NotFoundException("Not found");
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }

            throw new NotImplementedException();
        }

        public async Task<bool> SaveIntegrationSettings(MauticIntegrationDto integrationSettings, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(integrationSettings), Encoding.UTF8, "application/json");

            var response = string.IsNullOrEmpty(integrationSettings.Id)
                 ? await httpClient.PostAsync($"v1/Integrations/MauticSettings", content)
                 : await httpClient.PutAsync($"v1/Integrations/MauticSettings", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<Response<TenantDto>>(await response.Content.ReadAsStringAsync());
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine(result?.Message);
                    return false;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}