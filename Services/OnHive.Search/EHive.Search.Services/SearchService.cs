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

namespace EHive.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchRepository searchRepository;
        private readonly SearchApiSettings searchApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public SearchService(ISearchRepository searchRepository, SearchApiSettings searchApiSettings, IMapper mapper)
        {
            this.searchRepository = searchRepository;
            this.searchApiSettings = searchApiSettings;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            return await GetByFilterAsync(filter, loggedUser?.User?.TenantId);
        }

        public async Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, string tenantId)
        {
            var result = await searchRepository.GetByFilterAsync(filter, tenantId);
            return mapper.Map<List<SearchResultDto>>(result.Itens);
        }

        public async Task<int> TriggerGathering(bool full)
        {
            var resultCount = 0;
            var result = new List<SearchResult>();
            foreach (var target in searchApiSettings.Targets)
            {
                var searchResults = await searchRepository.GetDataAsync(target);
                result.AddRange(searchResults);
            }
            if (result.Any())
            {
                await searchRepository.DeleteManyAsync(result.Where(s => !s.IsActive).Select(s => s.Id).ToList());
                var saveResult = await searchRepository.SaveManyAsync(result.Where(s => s.IsActive).ToList());
                resultCount = saveResult.Count;
            }
            return resultCount;
        }

        public async Task<SearchResultDto> SaveAsync(SearchResultDto searchResultDto, LoggedUserDto? loggedUser)
        {
            var searchResult = mapper.Map<SearchResult>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            searchResult.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            searchResult.CreatedAt = DateTime.UtcNow;
            searchResult.CreatedBy = string.IsNullOrEmpty(searchResult.CreatedBy) ? loggedUser?.User?.Id : searchResult.CreatedBy;
            var response = await searchRepository.SaveAsync(searchResult);
            return mapper.Map<SearchResultDto>(response);
        }

        public async Task<SearchResultDto> CreateAsync(SearchResultDto searchResultDto, LoggedUserDto? loggedUser)
        {
            if (!searchResultDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var searchResult = mapper.Map<SearchResult>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            searchResult.Id = string.Empty;
            searchResult.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await searchRepository.SaveAsync(searchResult, loggedUser.User.Id);
            return mapper.Map<SearchResultDto>(response);
        }

        public async Task<SearchResultDto?> UpdateAsync(SearchResultDto searchResultDto, LoggedUserDto? loggedUser)
        {
            if (!searchResultDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var searchResult = mapper.Map<SearchResult>(searchResultDto);
            ValidatePermissions(searchResult, loggedUser?.User);
            var currentSearchResult = await searchRepository.GetByIdAsync(searchResult.Id);
            if (currentSearchResult == null || currentSearchResult.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await searchRepository.SaveAsync(searchResult, loggedUser.User.Id);
            return mapper.Map<SearchResultDto>(response);
        }

        private void ValidatePermissions(SearchResult searchResult, UserDto? loggedUser)
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