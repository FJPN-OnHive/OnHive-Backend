using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public class UserGroupsService : ServiceBase<UserGroupDto>, IUserGroupsService
    {
        public UserGroupsService(HttpClient httpClient) : base(httpClient, "/v1/UserGroup")
        {
        }
    }
}