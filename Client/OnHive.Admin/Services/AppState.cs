using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public class AppState
    {
        public LoggedUserDto? LoggedUser { get; set; }
        public TenantDto? Tenant { get; set; }
    }
}