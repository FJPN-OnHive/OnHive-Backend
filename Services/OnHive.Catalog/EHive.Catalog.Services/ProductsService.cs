using AutoMapper;
using EHive.Catalog.Domain.Abstractions.Repositories;
using EHive.Catalog.Domain.Abstractions.Services;
using EHive.Catalog.Domain.Models;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Domain.Exceptions;
using EHive.Core.Library.Entities.Catalog;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Extensions;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Validations.Common;
using EHive.Enrich.Library.Extensions;
using Serilog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EHive.Catalog.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IProductsRepository productsRepository;
        private readonly CatalogApiSettings catalogApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public ProductsService(IProductsRepository productsRepository,
                               CatalogApiSettings catalogApiSettings,
                               IMapper mapper,
                               HttpClient httpClient)
        {
            this.productsRepository = productsRepository;
            this.catalogApiSettings = catalogApiSettings;
            this.mapper = mapper;
            this.httpClient = httpClient;
            logger = Log.Logger;
        }

        public async Task<ProductDto?> GetByIdAsync(string productId)
        {
            var product = await productsRepository.GetByIdAsync(productId);
            var result = mapper.Map<ProductDto>(product);
            if (result != null)
            {
                result = await GetItem(result, null);
            }
            await result?.LoadEnrichmentAsync();
            return result;
        }

        public async Task<ProductDto?> GetByIdAsync(string productId, LoggedUserDto? loggedUser)
        {
            var product = await productsRepository.GetByIdAsync(productId);
            var result = mapper.Map<ProductDto>(product);
            if (result != null)
            {
                result = await GetItem(result, loggedUser);
            }
            await result?.LoadEnrichmentAsync();
            return result;
        }

        public async Task<ProductDto?> GetBySkuAsync(string sku, LoggedUserDto? loggedUser)
        {
            if (string.IsNullOrEmpty(loggedUser?.User?.TenantId)) throw new ArgumentException(nameof(loggedUser.User.TenantId));
            var product = await productsRepository.GetBySku(loggedUser.User.TenantId, sku);
            var result = mapper.Map<ProductDto>(product);
            if (result != null)
            {
                result = await GetItem(result, loggedUser);
            }
            await result?.LoadEnrichmentAsync();
            return result;
        }

        public async Task<ProductDto?> GetBySkuAsync(string sku, string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException(nameof(tenantId));
            var product = await productsRepository.GetBySku(tenantId, sku);
            var result = mapper.Map<ProductDto>(product);
            if (result != null)
            {
                result = await GetItem(result, null);
            }
            await result?.LoadEnrichmentAsync();
            return result;
        }

        public async Task<ProductDto> GetBySlugAsync(string slug, string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException(nameof(tenantId));
            var product = await productsRepository.GetBySlug(tenantId, slug);
            if (product == null)
            {
                product = await productsRepository.GetByAlternativeSlug(slug, tenantId);
                if (product == null)
                {
                    return null;
                }
                throw new RedirectException("Product", string.Empty, product.Slug, "301");
            }
            var result = mapper.Map<ProductDto>(product);
            if (result != null)
            {
                result = await GetItem(result, null);
            }
            await result?.LoadEnrichmentAsync();
            return result;
        }

        public async Task<PaginatedResult<ProductDto>> GetByFilterAsync(RequestFilter filter, string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId)) throw new ArgumentException(nameof(tenantId));
            var result = await productsRepository.GetByFilterActiveAsync(filter, tenantId);
            if (result != null)
            {
                var paginatedResult = new PaginatedResult<ProductDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<ProductDto>>(result.Itens)
                };
                paginatedResult.Itens.ForEach(async i => await i.LoadEnrichmentAsync());
                return paginatedResult;
            }
            return new PaginatedResult<ProductDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<ProductDto>()
            };
        }

        public async Task<PaginatedResult<ProductDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            if (string.IsNullOrEmpty(loggedUser?.User?.TenantId)) throw new ArgumentException(nameof(loggedUser.User.TenantId));
            var result = await productsRepository.GetByFilterAsync(filter, loggedUser.User.TenantId, false);
            if (result != null)
            {
                var paginatedResult = new PaginatedResult<ProductDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<ProductDto>>(result.Itens)
                };
                paginatedResult.Itens.ForEach(async i => await i.LoadEnrichmentAsync());
                return paginatedResult;
            }
            return new PaginatedResult<ProductDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<ProductDto>()
            };
        }

        public async Task<FilterScope> GetFilterScopeAsync(string tenantId)
        {
            return await productsRepository.GetFilterDataAsync(tenantId);
        }

        public async Task<PaginatedResult<ProductDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId)
        {
            var result = await GetByFilterAsync(filter, tenantId);
            result.Itens.ForEach(p =>
            {
                p.Prices = null;
                p.Item = null;
                p.StartDate = null;
                p.EndDate = null;
            });
            return result;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var products = await productsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            var result = mapper.Map<IEnumerable<ProductDto>>(products).ToList();
            result.ForEach(async i => await i.LoadEnrichmentAsync());
            return result;
        }

        public async Task<List<ProductDto>> GetByIdsAsync(List<string> productIds)
        {
            var products = await productsRepository.GetByIdsAsync(productIds);
            var result = mapper.Map<IEnumerable<ProductDto>>(products).ToList();
            result.ForEach(i => i.LoadEnrichmentAsync());
            return result;
        }

        public async Task<List<ProductDto>> GetByItemIdsAsync(List<string> itensIds)
        {
            var products = await productsRepository.GetByItemIdsAsync(itensIds);
            var result = mapper.Map<IEnumerable<ProductDto>>(products).ToList();
            result.ForEach(async i => await i.LoadEnrichmentAsync());
            return result;
        }

        public async Task<ProductDto> SaveAsync(ProductDto productDto, LoggedUserDto? loggedUser)
        {
            if (!productDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            if (string.IsNullOrEmpty(productDto.Sku))
            {
                productDto.Sku = CodeHelper.GenerateNumericCode(11);
            }
            if (string.IsNullOrEmpty(productDto.Slug))
            {
                productDto.Slug = productDto.Name.NormalizeSlug();
            }
            var product = mapper.Map<Product>(productDto);
            ValidatePermissions(product, loggedUser?.User);
            product.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            product.CreatedAt = DateTime.UtcNow;
            product.CreatedBy = string.IsNullOrEmpty(product.CreatedBy) ? loggedUser.User.Id : product.CreatedBy;
            product.LowPrice = product.Prices.Any() ? product.Prices.Min(p => p.Price) : product.FullPrice;
            var response = await productsRepository.SaveAsync(product);
            await productDto.SaveEnrichmentAsync();
            var result = mapper.Map<ProductDto>(response);
            result.CustomAttributes = productDto.CustomAttributes;
            return result;
        }

        public async Task<ProductDto> CreateAsync(ProductDto productDto, LoggedUserDto? loggedUser)
        {
            if (!productDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            if (string.IsNullOrEmpty(productDto.Sku))
            {
                productDto.Sku = CodeHelper.GenerateNumericCode(11);
            }
            if (string.IsNullOrEmpty(productDto.Slug))
            {
                productDto.Slug = productDto.Name.NormalizeSlug();
            }
            var product = mapper.Map<Product>(productDto);
            ValidatePermissions(product, loggedUser?.User);
            product.Id = string.Empty;
            product.TenantId = loggedUser?.User?.TenantId ?? string.Empty;
            product.LowPrice = product.Prices.Any() ? product.Prices.Min(p => p.Price) : product.FullPrice;
            var response = await productsRepository.SaveAsync(product, loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser.User.Id)));
            await productDto.SaveEnrichmentAsync();
            var result = mapper.Map<ProductDto>(response);
            result.CustomAttributes = productDto.CustomAttributes;
            return result;
        }

        public async Task<ProductDto?> UpdateAsync(ProductDto productDto, LoggedUserDto? loggedUser)
        {
            if (!productDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var product = mapper.Map<Product>(productDto);
            ValidatePermissions(product, loggedUser?.User);
            var currentProduct = await productsRepository.GetByIdAsync(product.Id);
            if (currentProduct == null || currentProduct.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            product.LowPrice = product.Prices.Any() ? product.Prices.Min(p => p.Price) : product.FullPrice;
            var response = await productsRepository.SaveAsync(product, loggedUser.User.Id);
            await productDto.SaveEnrichmentAsync();
            var result = mapper.Map<ProductDto>(response);
            result.CustomAttributes = productDto.CustomAttributes;
            return result;
        }

        public async Task<ProductDto?> PatchAsync(JsonDocument patch, LoggedUserDto? loggedUser)
        {
            var currentProduct = await productsRepository.GetByIdAsync(patch.GetId());
            if (currentProduct == null || currentProduct.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var newProduct = patch.PatchEntity(currentProduct);
            if (!mapper.Map<ProductDto?>(newProduct).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            ValidatePermissions(newProduct, loggedUser?.User);
            newProduct.LowPrice = newProduct.Prices.Any() ? newProduct.Prices.Min(p => p.Price) : newProduct.FullPrice;
            var response = await productsRepository.SaveAsync(newProduct, loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser.User.Id)));
            var result = mapper.Map<ProductDto>(response);
            await result.LoadEnrichmentAsync();
            return result;
        }

        public async Task<Stream> GetExportData(ExportFormats format, string tenantId, bool activeOnly)
        {
            var products = activeOnly
                ? await productsRepository.GetAllActive(tenantId)
                : await productsRepository.GetAllAsync(tenantId);

            var stream = format switch
            {
                ExportFormats.Csv => ToCsvStream(products),
                ExportFormats.Json => ToJsonStream(products),
                ExportFormats.Xml => ToXmlStream(products),
                ExportFormats.GoogleXml => ToGoogleXmlStream(products),
                ExportFormats.GoogleTsv => ToGoogleTsvStream(products),
                _ => throw new NotImplementedException()
            };
            return stream;
        }

        public async Task RefreshPrices(LoggedUserDto loggedUser)
        {
            var products = await productsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            foreach (var product in products)
            {
                product.LowPrice = product.Prices.Any(p => p.IsActive) ? product.Prices.Where(p => p.IsActive).Min(p => p.Price) : product.FullPrice;
                await productsRepository.SaveAsync(product, loggedUser.User.Id);
            }
        }

        private Stream ToGoogleTsvStream(List<Product> products)
        {
            var result = $"id\ttitle\tdescription\tavability\tlink\timage link\tprice\tidentifier exists\tbrand\n";
            foreach (var product in products)
            {
                var price = "0.0 BRL";
                if (product.Prices.Any(p => p.IsActive))
                {
                    price = (product.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToString().Replace(",", ".") + " BRL";
                }
                var itemUrl = string.IsNullOrEmpty(product.ItemUrl) ? $"https://www.fenixeducacao.org.br/curso/{product.Slug}" : product.ItemUrl;
                result += $"{product.Sku.Replace("\t", " ")}\t{product.Name.Replace("\t", " ")}\t{product.Description.Replace("\t", " ")}\tin_stock\t{itemUrl.Replace("\t", " ")}\t{product.ImageUrl.Replace("\t", " ")}\t{price.Replace("\t", " ")}\tno\tFenix Educa\u00e7\u00e3o\n";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream ToGoogleXmlStream(List<Product> products)
        {
            var resultXml = $@"<?xml version=""1.0""?>
                               <rss xmlns:g=""http://base.google.com/ns/1.0"" version=""2.0"">
                                   <channel>
                                       <title>Fenix Educa��o - Produtos</title>
                                       <link>https://fenixeducacao.org.br</link>
                                       <description></description>
                                       {GetGoogleXmlItens(products)}
                                   </channel>
                               </rss>";

            return new MemoryStream(Encoding.UTF8.GetBytes(resultXml));
        }

        private string GetGoogleXmlItens(List<Product> products)
        {
            var result = "";

            foreach (var product in products)
            {
                var price = "0.0 BRL";
                if (product.Prices.Any(p => p.IsActive))
                {
                    price = (product.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToString().Replace(",", ".") + " BRL";
                }
                var itemUrl = string.IsNullOrEmpty(product.ItemUrl) ? $"https://www.fenixeducacao.org.br/curso/{product.Slug}" : product.ItemUrl;
                result += @$"<item>
                                <g:id>{product.Sku.EscapeXml()}</g:id>
                                <g:title>{product.Name.EscapeXml()}</g:title>
                                <g:description>{product.Description.EscapeXml()}</g:description>
                                <g:link>{itemUrl.EscapeXml()}</g:link>
                                <g:image_link>{product.ImageUrl.EscapeXml()}</g:image_link>
                                <g:availability>in_stock</g:availability>
                                <g:price>{price.EscapeXml()}</g:price>
                                <g:identifier_exists>no</g:identifier_exists>
                                <g:brand>Fenix Educa��o</g:brand>
                            </item>
                            ";
            }
            return result;
        }

        private Stream ToXmlStream(List<Product> products)
        {
            var resultXml = $@"<?xml version=""1.0""?>
                               <products>
                                   {GetXmlItens(products)}
                               </products>";
            return new MemoryStream(Encoding.UTF8.GetBytes(resultXml));
        }

        private string GetXmlItens(List<Product> products)
        {
            var result = "";
            foreach (var product in products)
            {
                var price = "0.0 BRL";
                if (product.Prices.Any(p => p.IsActive))
                {
                    price = (product.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToString().Replace(",", ".") + " BRL";
                }
                var itemUrl = string.IsNullOrEmpty(product.ItemUrl) ? $"https://www.fenixeducacao.org.br/curso/{product.Slug}" : product.ItemUrl;
                result += @$"<product>
                                <sku>{product.Sku.EscapeXml()}</sku>
                                <name>{product.Name.EscapeXml()}</name>
                                <description>{product.Description.EscapeXml()}</description>
                                <itemUrl>{itemUrl.EscapeXml()}</itemUrl>
                                <imageUrl>{product.ImageUrl.EscapeXml()}</imageUrl>
                                <price>{price.EscapeXml()}</price>
                            </product>
                            ";
            }
            return result;
        }

        private Stream ToJsonStream(List<Product> products)
        {
            var result = JsonSerializer.Serialize(products);

            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream ToCsvStream(List<Product> products)
        {
            var result = $"id;sku;name;description;link;Image Url;price;slug;tenantId;active\n";
            foreach (var product in products)
            {
                var price = "0.0 BRL";
                if (product.Prices.Any(p => p.IsActive))
                {
                    price = (product.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToString().Replace(",", ".");
                }
                var itemUrl = string.IsNullOrEmpty(product.ItemUrl) ? $"https://www.fenixeducacao.org.br/curso/{product.Slug}" : product.ItemUrl;
                var active = product.IsActive ? "yes" : "no";
                result += $"{product.Id.Replace(";", " ")};{product.Sku.Replace(";", " ")};{product.Name.Replace(";", " ")};{product.Description.Replace(";", " ")};{itemUrl.Replace(";", " ")};{product.ImageUrl.Replace(";", " ")};{price} BRL;{product.Slug.Replace(";", " ")};{product.TenantId.Replace(";", " ")};{active}\n";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private async Task<ProductDto> GetItem(ProductDto product, LoggedUserDto? loggedUser)
        {
            if (!string.IsNullOrEmpty(product.ItemId) && !string.IsNullOrEmpty(product.ItemType))
            {
                var itemType = catalogApiSettings.ProductTypes
                    .FirstOrDefault(i => i.Key.Equals(product.ItemType, StringComparison.InvariantCultureIgnoreCase),
                        catalogApiSettings.ProductTypes.FirstOrDefault(d => d.IsDefault));
                if (itemType != null)
                {
                    if (loggedUser != null)
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loggedUser.Token);
                    }
                    var response = await httpClient.GetAsync($"{itemType.BaseUrl}{product.ItemId}");
                    if (response.IsSuccessStatusCode)
                    {
                        product.Item = JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            return product;
        }

        private void ValidatePermissions(Product product, UserDto? loggedUser)
        {
            if (loggedUser != null && product.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Product/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    product.Id, product.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}