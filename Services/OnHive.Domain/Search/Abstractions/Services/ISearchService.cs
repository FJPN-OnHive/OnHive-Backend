using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Search;
using EHive.Core.Library.Contracts.Login;

namespace EHive.Search.Domain.Abstractions.Services
{
    public interface ISearchService
    {
        Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<int> TriggerGathering(bool full);
    }
}