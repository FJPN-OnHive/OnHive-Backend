using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Search;
using OnHive.Core.Library.Contracts.Login;

namespace OnHive.Search.Domain.Abstractions.Services
{
    public interface ISearchService
    {
        Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<List<SearchResultDto>> GetByFilterAsync(RequestFilter filter, string tenantId);

        Task<int> TriggerGathering(bool full);
    }
}