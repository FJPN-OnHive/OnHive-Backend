using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public interface IRolesService : IServiceBase<RoleDto>
    {
        Task<List<string>> GetPermissions(string token);
    }
}