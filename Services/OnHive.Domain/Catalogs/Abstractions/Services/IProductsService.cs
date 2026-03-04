using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Enums.Common;
using System.Text.Json;

namespace OnHive.Catalog.Domain.Abstractions.Services
{
    public interface IProductsService
    {
        Task<ProductDto?> GetByIdAsync(string productId);

        Task<ProductDto?> GetByIdAsync(string productId, LoggedUserDto? loggedUser);

        Task<ProductDto?> GetBySkuAsync(string sku, LoggedUserDto? loggedUser);

        Task<ProductDto?> GetBySkuAsync(string sku, string tenantId);

        Task<ProductDto> GetBySlugAsync(string slug, string tenantId);

        Task<PaginatedResult<ProductDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<PaginatedResult<ProductDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<FilterScope> GetFilterScopeAsync(string tenantId);

        Task<IEnumerable<ProductDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<ProductDto> SaveAsync(ProductDto productDto, LoggedUserDto? user);

        Task<ProductDto> CreateAsync(ProductDto productDto, LoggedUserDto loggedUser);

        Task<ProductDto?> UpdateAsync(ProductDto productDto, LoggedUserDto loggedUser);

        Task<ProductDto?> PatchAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<PaginatedResult<ProductDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId);

        Task<Stream> GetExportData(ExportFormats format, string tenantId, bool activeOnly);

        Task<List<ProductDto>> GetByIdsAsync(List<string> productIds);

        Task<List<ProductDto>> GetByItemIdsAsync(List<string> itensIds);

        Task RefreshPrices(LoggedUserDto loggedUser);
    }
}