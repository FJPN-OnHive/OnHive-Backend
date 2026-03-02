using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Entities.Search;
using EHive.Search.Domain.Models;

namespace EHive.Search.Domain.Abstractions.Repositories
{
    public interface ISearchRepository : IRepositoryBase<SearchResult>
    {
        Task<List<SearchResult>> GetDataAsync(Target target);
    }
}