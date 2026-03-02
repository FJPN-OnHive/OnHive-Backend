using EHive.Core.Library.Contracts.Search;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public interface ISearchService : IServiceBase<SearchResultDto>
    {
        public Task<int> TriggerProductCourseSearch(bool full, string token);

        public Task<int> TriggerSearch(string token);
    }
}