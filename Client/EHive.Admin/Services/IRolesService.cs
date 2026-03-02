using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public interface IRolesService : IServiceBase<RoleDto>
    {
        Task<List<string>> GetPermissions(string token);
    }
}