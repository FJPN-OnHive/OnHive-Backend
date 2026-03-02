using EHive.Core.Library.Contracts.Certificates;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Exceptions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class CertificatesService : ServiceBase<CertificateDto>, ICertificatesService
    {
        public CertificatesService(HttpClient httpClient) : base(httpClient, "/v1/Certificate")
        {
        }

        public async Task<CertificateMountDto?> GetEmmitedCertificateById(string certificateId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"/v1/Emmited/Certificate/{certificateId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<CertificateMountDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Payload;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicatedException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<CertificateMountDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new HttpRequestException(result?.Message);
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}