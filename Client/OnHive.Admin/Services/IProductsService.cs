using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Enums.Common;

namespace OnHive.Admin.Services
{
    public interface IProductsService : IServiceBase<ProductDto>
    {
        string GetExportProducUrl(ExportFormats exportType, string tenantId, bool activeOnly);

        Task<string> RefreshProductsPricesAsync(string token);
    }
}