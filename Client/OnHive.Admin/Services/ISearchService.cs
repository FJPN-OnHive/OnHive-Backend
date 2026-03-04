using OnHive.Core.Library.Contracts.Search;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public interface ISearchService : IServiceBase<SearchResultDto>
    {
        public Task<int> TriggerProductCourseSearch(bool full, string token);

        public Task<int> TriggerSearch(string token);
    }
}