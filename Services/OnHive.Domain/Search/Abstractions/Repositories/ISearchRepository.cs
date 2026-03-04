using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Entities.Search;
using OnHive.Search.Domain.Models;

namespace OnHive.Search.Domain.Abstractions.Repositories
{
    public interface ISearchRepository : IRepositoryBase<SearchResult>
    {
        Task<List<SearchResult>> GetDataAsync(Target target);
    }
}