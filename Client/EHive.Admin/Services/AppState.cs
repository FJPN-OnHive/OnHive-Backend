using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Admin.Services
{
    public class AppState
    {
        public LoggedUserDto? LoggedUser { get; set; }
        public TenantDto? Tenant { get; set; }
    }
}