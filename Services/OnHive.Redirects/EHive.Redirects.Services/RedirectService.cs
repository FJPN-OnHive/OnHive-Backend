using AutoMapper;
using EHive.Core.Library.Contracts.Redirects;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Redirects;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Redirects.Domain.Abstractions.Repositories;
using EHive.Redirects.Domain.Abstractions.Services;
using EHive.Redirects.Domain.Models;
using Serilog;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Entities.Courses;
using System.Text.Json;
using System.Text;
using EHive.Core.Library.Extensions;

namespace EHive.Redirects.Services
{
    public class RedirectService : IRedirectService
    {
        private readonly IRedirectRepository redirectRepository;
        private readonly RedirectApiSettings redirectApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public RedirectService(IRedirectRepository redirectRepository, RedirectApiSettings redirectApiSettings, IMapper mapper, HttpClient httpClient)
        {
            this.redirectRepository = redirectRepository;
            this.redirectApiSettings = redirectApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<RedirectDto?> ExecuteRedirect(string tenantId, string path)
        {
            var redirect = await redirectRepository.GetByPathAsync(tenantId, path);
            if (redirect == null)
            {
                return null;
            }
            return mapper.Map<RedirectDto>(redirect);
        }

        public async Task<RedirectDto?> GetByIdAsync(string redirectId)
        {
            var redirect = await redirectRepository.GetByIdAsync(redirectId);
            return mapper.Map<RedirectDto>(redirect);
        }

        public async Task<PaginatedResult<RedirectDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await redirectRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            if (result != null)
            {
                return new PaginatedResult<RedirectDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<RedirectDto>>(result.Itens)
                };
            }
            return new PaginatedResult<RedirectDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<RedirectDto>()
            };
        }

        public async Task<IEnumerable<RedirectDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var redirects = await redirectRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<RedirectDto>>(redirects);
        }

        public async Task<RedirectDto> SaveAsync(RedirectDto redirectDto, LoggedUserDto? loggedUser)
        {
            var redirect = mapper.Map<Redirect>(redirectDto);
            ValidatePermissions(redirect, loggedUser?.User);
            redirect.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            redirect.CreatedAt = DateTime.UtcNow;
            redirect.IsActive = true;
            redirect.CreatedBy = string.IsNullOrEmpty(redirect.CreatedBy) ? loggedUser?.User?.Id : redirect.CreatedBy;

            var response = await redirectRepository.SaveAsync(redirect);
            return mapper.Map<RedirectDto>(response);
        }

        public async Task<RedirectDto> CreateAsync(RedirectDto redirectDto, LoggedUserDto? loggedUser)
        {
            if (!redirectDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var redirect = mapper.Map<Redirect>(redirectDto);
            ValidatePermissions(redirect, loggedUser?.User);
            redirect.Id = string.Empty;
            redirect.IsActive = true;
            redirect.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await redirectRepository.SaveAsync(redirect, loggedUser.User.Id);
            return mapper.Map<RedirectDto>(response);
        }

        public async Task<RedirectDto?> UpdateAsync(RedirectDto redirectDto, LoggedUserDto? loggedUser)
        {
            if (!redirectDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var redirect = mapper.Map<Redirect>(redirectDto);
            ValidatePermissions(redirect, loggedUser?.User);
            var currentRedirect = await redirectRepository.GetByIdAsync(redirect.Id);
            if (currentRedirect == null || currentRedirect.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await redirectRepository.SaveAsync(redirect, loggedUser.User.Id);
            return mapper.Map<RedirectDto>(response);
        }

        public async Task<bool> DeleteAsync(string redirectId, LoggedUserDto loggedUser)
        {
            var redirect = await redirectRepository.GetByIdAsync(redirectId);
            if (redirect == null)
            {
                return false;
            }
            ValidatePermissions(redirect, loggedUser?.User);
            return await redirectRepository.DeleteAsync(redirectId, loggedUser.User.Id);
        }

        private void ValidatePermissions(Redirect redirect, UserDto? loggedUser)
        {
            if (loggedUser != null && redirect.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Redirect/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    redirect.Id, redirect.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        public async Task<Stream> GetExportData(ExportFormats exportFormat, string tenantId, bool activeOnly)
        {
            var courses = activeOnly
               ? await redirectRepository.GetAllActive(tenantId)
               : await redirectRepository.GetAllAsync(tenantId);

            var stream = exportFormat switch
            {
                ExportFormats.Csv => ToCsvStream(courses),
                ExportFormats.Json => ToJsonStream(courses),
                ExportFormats.Xml => ToXmlStream(courses),
                _ => throw new NotImplementedException()
            };
            return stream;
        }

        private Stream ToXmlStream(List<Redirect> redirects)
        {
            var resultXml = $@"<?xml version=""1.0""?>
                               <redirects>
                                   {GetXmlItens(redirects)}
                               </redirects>";
            return new MemoryStream(Encoding.UTF8.GetBytes(resultXml));
        }

        private string GetXmlItens(List<Redirect> redirects)
        {
            var result = "";
            foreach (var redirect in redirects)
            {
                var active = redirect.IsActive ? "yes" : "no";
                result += @$"<redirect>
                                <id>{redirect.Id.EscapeXml()}</id>
                                <name>{redirect.Name.EscapeXml()}</name>
                                <description>{redirect.Description.EscapeXml()}</description>
                                <path>{redirect.Path.EscapeXml()}</path>
                                <redirectUrl>{redirect.RedirectUrl.EscapeXml()}</redirectUrl>
                                <type>{redirect.Type.ToString().EscapeXml()}</type>
                                <tenantId>{redirect.TenantId}</tenantId>
                                <active>{active}</active>
                            </redirect>
                            ";
            }
            return result;
        }

        private Stream ToJsonStream(List<Redirect> redirects)
        {
            var result = JsonSerializer.Serialize(redirects);
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream ToCsvStream(List<Redirect> redirects)
        {
            var result = $"id;name;description;path;redirect Url;type;tenantId;active\n";
            foreach (var redirect in redirects)
            {
                var active = redirect.IsActive ? "yes" : "no";
                result += $"{redirect.Id.Replace(";", " ")};{redirect.Name.Replace(";", " ")};{redirect.Description.Replace(";", " ")};{redirect.Path.Replace(";", " ")};{redirect.RedirectUrl.Replace(";", " ")};{redirect.Type.ToString().Replace(";", " ")};{redirect.TenantId.Replace(";", " ")};{active}\n";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }
    }
}