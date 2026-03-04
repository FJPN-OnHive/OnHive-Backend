using OnHive.Admin.Models;
using OnHive.Admin.Pages;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnHive.Admin.Services
{
    public class StudentsService : ServiceBase<StudentDto>, IStudentsService
    {
        public StudentsService(HttpClient httpClient) : base(httpClient, "/v1/Student")
        {
        }

        public async Task<PaginatedResult<StudentUserDto>> GetStudentUsersPaginated(RequestFilter filter, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}Users?{filter.ToString()}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<StudentUserDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload ?? new PaginatedResult<StudentUserDto>();
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

        public async Task<StudentDto> EnrollAsync(EnrollmentMessage enrollmentMessage, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(enrollmentMessage), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{baseUrl}/Enroll", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StudentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                var result = JsonSerializer.Deserialize<Response<StudentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new HttpRequestException(result?.Message);
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<List<SyntheticEnrollmentDto>> GetEnrollmentsSyntheticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var url = $"{baseUrl}s/Enrollments/Synthetic";
            if (initialDate != DateTime.MinValue)
            {
                url += $"?initial_date={initialDate:yyyy-MM-dd}";
            }
            if (finalDate != DateTime.MinValue)
            {
                url += (url.Contains("?") ? "&" : "?") + $"final_date={finalDate:yyyy-MM-dd}";
            }
            if (courses != null && courses.Count > 0)
            {
                url += (url.Contains("?") ? "&" : "?") + $"courses={string.Join(",", courses)}";
            }
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<List<SyntheticEnrollmentDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                var result = JsonSerializer.Deserialize<Response<SyntheticEnrollmentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new HttpRequestException(result?.Message);
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<StudentReportDto> GetEnrollmentsAnalyticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, string token)
        {
            var client = new HttpClient();
            client.BaseAddress = httpClient.BaseAddress;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            client.Timeout = TimeSpan.FromMinutes(10);
            var url = $"{baseUrl}s/Enrollments/AsyncAnalytic";
            if (initialDate != DateTime.MinValue)
            {
                url += $"?initial_date={initialDate:yyyy-MM-dd}";
            }
            if (finalDate != DateTime.MinValue)
            {
                url += (url.Contains("?") ? "&" : "?") + $"final_date={finalDate:yyyy-MM-dd}";
            }
            if (courses != null && courses.Count > 0)
            {
                url += (url.Contains("?") ? "&" : "?") + $"courses={string.Join(",", courses)}";
            }
            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StudentReportDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<StudentDto> UnenrollAsync(string studentId, string courseId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.PutAsync($"{baseUrl}/UnEnroll/{studentId}/{courseId}", null);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StudentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                var result = JsonSerializer.Deserialize<Response<StudentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new HttpRequestException(result?.Message);
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task ReemitCertificateAsync(string courseId, string userId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/EmmitCertificate/{userId}/{courseId}?reemmit=true");
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return;
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
                var result = JsonSerializer.Deserialize<Response<StudentDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new HttpRequestException(result?.Message);
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<StudentReportDto> GetSurveyAnalyticAsync(DateTime initialDate, DateTime finalDate, List<string> courses, bool isSatisfaction, string token)
        {
            var client = new HttpClient();
            client.BaseAddress = httpClient.BaseAddress;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            client.Timeout = TimeSpan.FromMinutes(10);
            var url = $"{baseUrl}s/Surveys/AsyncAnalytic";
            if (isSatisfaction)
            {
                url += "/Final";
            }
            else
            {
                url += "/Initial";
            }
            if (initialDate != DateTime.MinValue)
            {
                url += $"?initial_date={initialDate:yyyy-MM-dd}";
            }
            if (finalDate != DateTime.MinValue)
            {
                url += (url.Contains("?") ? "&" : "?") + $"final_date={finalDate:yyyy-MM-dd}";
            }
            if (courses != null && courses.Count > 0)
            {
                url += (url.Contains("?") ? "&" : "?") + $"courses={string.Join(",", courses)}";
            }
            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StudentReportDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}