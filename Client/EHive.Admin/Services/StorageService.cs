using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Storages;
using EHive.Core.Library.Exceptions;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EHive.Admin.Services
{
    public class StorageService : ServiceBase<StorageFileDto>, IStorageService
    {
        public StorageService(HttpClient httpClient) : base(httpClient, "v1/Storage")
        {
        }

        public async Task<bool> DeleteFileAsync(string id, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.DeleteAsync($"{baseUrl}/File/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
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

        public async Task<StorageFileDto> UploadFileAsync(StorageFileDto file, Stream streamFile, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            var content = new MultipartFormDataContent
            {
                { new StreamContent(streamFile), "file", file.OriginalFileName },
                { new StringContent(file.Name, Encoding.UTF8, "application/json"), "name" },
                { new StringContent(file.Description, Encoding.UTF8, "application/json"), "description" },
                { new StringContent(string.Join(",", file.Tags)), "tags" },
                { new StringContent(string.Join(",", file.Categories)), "categories" },
                { new StringContent(file.Public.ToString()), "public" }
            };

            var response = await httpClient.PostAsync($"{baseUrl}/File", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StorageFileDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<List<StorageFileDto>> GetFilesAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/Files");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<StorageFileDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload?.Itens ?? [];
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

        public async Task<StorageFileDto> UpdateFileAsync(StorageFileDto file, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(file), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{baseUrl}/File", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StorageFileDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<bool> DeleteImageAsync(string id, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.DeleteAsync($"{baseUrl}/Image/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
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

        public async Task<List<StorageImageFileDto>> GetImagesAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/Images");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<PaginatedResult<StorageImageFileDto>>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Code == Core.Library.Enums.Common.ResponseCode.OK)
                {
                    return result.Payload?.Itens ?? [];
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

        public async Task<StorageImageFileDto> UpdateImageAsync(StorageImageFileDto file, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var content = new StringContent(JsonSerializer.Serialize(file), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{baseUrl}/Image", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StorageImageFileDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<StorageImageFileDto> UploadImageAsync(StorageImageFileDto file, Stream image, bool noConvert, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            var content = new MultipartFormDataContent
            {
                { new StreamContent(image), "file", file.OriginalFileName },
                { new StringContent(file.Name, Encoding.UTF8, "application/json"), "imageId" },
                { new StringContent(file.AltText, Encoding.UTF8, "application/json"), "altText" },
                { new StringContent(file.Subtitle, Encoding.UTF8, "application/json"), "subtitle" },
                { new StringContent(string.Join(",", file.Tags)), "tags" },
                { new StringContent(string.Join(",", file.Categories)), "categories" },
                { new StringContent(file.Public.ToString()), "public" },
                { new StringContent(noConvert.ToString()), "noConvert" }
            };

            var response = await httpClient.PostAsync($"{baseUrl}/Image", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Response<StorageImageFileDto>>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<Stream> GetPrivateFileStreamAsync(string fileId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.GetAsync($"{baseUrl}/PrivateFile/{fileId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStreamAsync();
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

        public async Task<Stream> GetPublicFileStreamAsync(string fileId, string tenantId)
        {
            var response = await httpClient.GetAsync($"{baseUrl}/File/{tenantId}/{fileId}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStreamAsync();
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