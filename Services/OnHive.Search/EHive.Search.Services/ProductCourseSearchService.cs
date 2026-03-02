using AutoMapper;
using EHive.Core.Library.Contracts.Search;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Search;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Search.Domain.Abstractions.Repositories;
using EHive.Search.Domain.Abstractions.Services;
using EHive.Search.Domain.Models;
using Serilog;
using System.Diagnostics;

namespace EHive.Search.Services
{
    public class ProductCourseSearchService : IProductCourseSearchService
    {
        private readonly IProductCourseSearchRepository searchRepository;
        private readonly SearchApiSettings searchApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ProductCourseSearchService(IProductCourseSearchRepository searchRepository, SearchApiSettings searchApiSettings, IMapper mapper)
        {
            this.searchRepository = searchRepository;
            this.searchApiSettings = searchApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<int> TriggerGathering(bool full)
        {
            var lowerDate = !full ? await searchRepository.GetLowerDate() : DateTime.MinValue;
            var courses = await searchRepository.GetUpdatedCourses(lowerDate);
            var products = await searchRepository.GetProductsByCourseIds(courses.Select(c => c.Id).ToList());
            var UpdatedProducts = await searchRepository.GetUpdatedProduct(lowerDate, products.Select(p => p.Id).ToList());
            courses.AddRange(await searchRepository.GetCourseByIds(UpdatedProducts.Select(p => p.ItemId).ToList()));
            products.AddRange(UpdatedProducts);
            if (!courses.Any() && !products.Any())
            {
                return 0;
            }
            var searchResults = new List<ProductCourseSearch>();
            foreach (var course in courses)
            {
                var relativeProducts = products.Where(p => p.ItemId == course.Id);
                foreach (var product in relativeProducts)
                {
                    var productCourseSearch = new ProductCourseSearch
                    {
                        Id = course.Id,
                        CourseId = course.Id,
                        ProductId = product.Id,
                        Name = course.Name,
                        Description = course.Description,
                        TenantId = course.TenantId,
                        CreatedAt = course.CreatedAt,
                        CreatedBy = course.CreatedBy,
                        UpdatedBy = course.UpdatedBy,
                        Thumbnail = course.Thumbnail,
                        ImageUrl = course.ImageUrl,
                        ItemUrl = product.ItemUrl,
                        Url = course.Url,
                        Category = course.Category,
                        Categories = course.Categories,
                        Tags = string.Join(",", course.Tags),
                        LaunchDate = course.LaunchDate,
                        Code = course.Code,
                        TotalTimeMinutes = course.Duration * 60,
                        Rate = course.Rate,
                        DifficultLevel = course.DifficultLevel,
                        Slug = string.IsNullOrEmpty(course.Slug) ? product.Slug : course.Slug,
                        FullPrice = product.FullPrice,
                        LowPrice = product.LowPrice,
                        Sales = product.Sales,
                        IsActive = course.IsActive && product.IsActive,
                        SnapshotDate = product.UpdatedAt > course.UpdatedAt ? product.UpdatedAt : course.UpdatedAt,
                        UpdatedAt = product.UpdatedAt > course.UpdatedAt ? product.UpdatedAt : course.UpdatedAt,
                        Prices = product.Prices,
                        ExternalUrl = product.ItemUrl
                    };
                    searchResults.Add(productCourseSearch);
                }
            }
            if (searchResults.Count == 0)
            {
                return 0;
            }
            await searchRepository.SaveManyAsync(searchResults);
            return searchResults.Count;
        }

        public async Task<FilterScope> GetFilterScopeAsync(string tenantId)
        {
            return await searchRepository.GetFilterDataAsync(tenantId);
        }

        public async Task<PaginatedResult<ProductCourseSearchDto>> GetByFilterAsync(RequestFilter filter, string tenantId)
        {
            var result = await searchRepository.GetByFilterAsync(filter, tenantId);
            if (result != null)
            {
                return new PaginatedResult<ProductCourseSearchDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<ProductCourseSearchDto>>(result.Itens)
                };
            }
            return new PaginatedResult<ProductCourseSearchDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<ProductCourseSearchDto>()
            };
        }

        public async Task<ProductCourseSearchDto> SaveAsync(ProductCourseSearchDto searchResultDto, LoggedUserDto? loggedUser)
        {
            var searchResult = mapper.Map<ProductCourseSearch>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            searchResult.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            searchResult.CreatedAt = DateTime.UtcNow;
            searchResult.CreatedBy = string.IsNullOrEmpty(searchResult.CreatedBy) ? loggedUser?.User?.Id : searchResult.CreatedBy;
            var response = await searchRepository.SaveAsync(searchResult);
            return mapper.Map<ProductCourseSearchDto>(response);
        }

        public async Task<ProductCourseSearchDto> CreateAsync(ProductCourseSearchDto searchResultDto, LoggedUserDto? loggedUser)
        {
            if (!searchResultDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var searchResult = mapper.Map<ProductCourseSearch>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            searchResult.Id = string.Empty;
            searchResult.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await searchRepository.SaveAsync(searchResult, loggedUser.User.Id);
            return mapper.Map<ProductCourseSearchDto>(response);
        }

        public async Task<ProductCourseSearchDto?> UpdateAsync(ProductCourseSearchDto searchResultDto, LoggedUserDto? loggedUser)
        {
            if (!searchResultDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var searchResult = mapper.Map<ProductCourseSearch>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            var currentSearchResult = await searchRepository.GetByIdAsync(searchResult.Id);
            if (currentSearchResult == null || currentSearchResult.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await searchRepository.SaveAsync(searchResult, loggedUser.User.Id);
            return mapper.Map<ProductCourseSearchDto>(response);
        }

        private void ValidatePermissions(ProductCourseSearch searchResult, UserDto? loggedUser)
        {
            if (loggedUser != null && searchResult.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID SearchResult/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    searchResult.Id, searchResult.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}