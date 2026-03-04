using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Redirects;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Admin.Services
{
    public interface IRedirectsService : IServiceBase<RedirectDto>
    {
        string GetExportRedirectsUrl(ExportFormats exportType, string tenantId, bool exportActiveOnly);
    }
}