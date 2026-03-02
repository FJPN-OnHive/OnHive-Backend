using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Enums.Common;

namespace EHive.Admin.Services
{
    public interface IProductsService : IServiceBase<ProductDto>
    {
        string GetExportProducUrl(ExportFormats exportType, string tenantId, bool activeOnly);

        Task<string> RefreshProductsPricesAsync(string token);
    }
}