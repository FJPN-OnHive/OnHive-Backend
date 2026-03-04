using BlazorMonaco;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Tenants;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class TenantsService : ITenantsService
    {
        private readonly HttpClient httpClient;

        public TenantsService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<TenantDto?> GetTenant(string tenantId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"/v1/Tenant/{tenantId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<Response<TenantDto>>(await response.Content.ReadAsStringAsync());
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<List<FeatureDto>> GetFeatures(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync("/v1/Features");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<Response<List<FeatureDto>>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload ?? [];
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<bool> SaveTenant(TenantDto tenant, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(tenant), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"/v1/Tenant", content);
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

        public async Task<List<TenantResumeDto>> GetTenantsResumesAsync()
        {
            var response = await httpClient.GetAsync($"/v1/Tenant/List");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JsonSerializer.Deserialize<Response<List<TenantResumeDto>>>(await response.Content.ReadAsStringAsync());
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}