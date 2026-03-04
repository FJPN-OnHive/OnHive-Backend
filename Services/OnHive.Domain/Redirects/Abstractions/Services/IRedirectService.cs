using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Redirects.Domain.Abstractions.Services
{
    public interface IRedirectService
    {
        Task<RedirectDto?> GetByIdAsync(string redirectId);

        Task<PaginatedResult<RedirectDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<RedirectDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<RedirectDto> SaveAsync(RedirectDto redirectDto, LoggedUserDto? user);

        Task<RedirectDto> CreateAsync(RedirectDto redirectDto, LoggedUserDto? loggedUser);

        Task<RedirectDto?> UpdateAsync(RedirectDto redirectDto, LoggedUserDto? loggedUser);

        Task<RedirectDto> ExecuteRedirect(string tenantId, string path);

        Task<bool> DeleteAsync(string redirectId, LoggedUserDto loggedUser);

        Task<Stream> GetExportData(ExportFormats exportFormat, string tenantId, bool v);
    }
}