using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class ExamsService : ServiceBase<ExamDto>, IExamsService
    {
        public ExamsService(HttpClient httpClient) : base(httpClient, "/v1/Exam")
        {
        }

        public async Task<ExamDto?> SaveVersion(ExamDto dto, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{baseUrl}/NewVersion", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<ExamDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicatedException();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<ExamDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new InvalidPayloadException(result?.Message.Split(";").ToList());
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}