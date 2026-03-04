using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public class UserGroupsService : ServiceBase<UserGroupDto>, IUserGroupsService
    {
        public UserGroupsService(HttpClient httpClient) : base(httpClient, "/v1/UserGroup")
        {
        }
    }
}