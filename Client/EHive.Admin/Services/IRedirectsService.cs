using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Redirects;
using EHive.Core.Library.Enums.Common;

namespace EHive.Admin.Services
{
    public interface IRedirectsService : IServiceBase<RedirectDto>
    {
        string GetExportRedirectsUrl(ExportFormats exportType, string tenantId, bool exportActiveOnly);
    }
}