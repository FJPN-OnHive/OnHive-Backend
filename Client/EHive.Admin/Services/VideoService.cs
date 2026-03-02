using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Storages;
using EHive.Core.Library.Contracts.Videos;
using EHive.Core.Library.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class VideoService : ServiceBase<VideoDto>, IVideoService
    {
        public VideoService(HttpClient httpClient)
            : base(httpClient, "v1") 
        {
        }
        
        public async Task<List<VideoDto>> GetVideosAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/Videos");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<VideoDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    Console.WriteLine($"Deserialized Payload: {JsonSerializer.Serialize(result.Payload?.Itens)}");
                    return result.Payload?.Itens ?? [];
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }

            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

       
       
       
        public async Task<VideoDto> CreateVideoAsync(VideoDto file, Stream video, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            var content = new MultipartFormDataContent
            {
                { new StreamContent(video), "file", file.FileName },
                { new StringContent(file.Name, Encoding.UTF8, "application/json"), "Name" },
                { new StringContent(file.Description, Encoding.UTF8, "application/json"), "Description" },
                { new StringContent(string.Join(",", file.Tags)), "tags" },
                { new StringContent(string.Join(",", file.Categories)), "categories" },
            };
            
            var response = await httpClient.PostAsync($"{baseUrl}/Video", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<VideoDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new DuplicatedException();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }


        public async Task<VideoDto> UpdateVideoAsync(VideoDto video, Stream videoStream, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            HttpResponseMessage response;

            if (videoStream == null)
            {
                var json = JsonSerializer.Serialize(video);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                response = await httpClient.PutAsync($"{baseUrl}/Video", content);
            }
            else
            {
                var content = new MultipartFormDataContent
        {
            { new StringContent(video.Id, Encoding.UTF8, "application/json"), "id" },
            { new StringContent(video.TenantId, Encoding.UTF8, "application/json"), "tenantId" },
            { new StringContent(video.Name, Encoding.UTF8, "application/json"), "Name" },
            { new StringContent(video.Description, Encoding.UTF8, "application/json"), "Description" },
            { new StringContent(string.Join(",", video.Tags)), "tags" },
            { new StringContent(string.Join(",", video.Categories)), "categories" },
        };

                content.Add(new StreamContent(videoStream), "file", video.FileName);

                response = await httpClient.PutAsync($"{baseUrl}/Video", content);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<VideoDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload;
                }
                else
                {
                    throw new HttpRequestException(result?.Message);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
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
