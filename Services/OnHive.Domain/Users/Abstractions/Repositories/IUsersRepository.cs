using EHive.Core.Library.Abstractions.Repositories;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Users;

namespace EHive.Users.Domain.Abstractions.Repositories
{
    public interface IUsersRepository : IRepositoryBase<User>
    {
        Task<User?> GetByMainEmailAsync(string email, string tenantId);

        Task<User?> GetByEmailAsync(string email, string tenantId);

        Task<User?> GetByLoginAsync(string login, string tenantId);

        Task<User?> GetByLoginCodeAsync(string code);

        Task<User?> GetByMainEmailCodeAsync(string code, string tenantId);

        Task<User?> GetByRecoverPasswordCodeAsync(string code, string tenantId);

        Task<PaginatedResult<User?>> GetByFilterAndProfileAsync(RequestFilter filter, ProfileTypes profile, string? tenantId, bool activeOnly = true);
    }
}