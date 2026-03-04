using OnHive.Core.Library.Abstractions.Repositories;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Enums.Users;

namespace OnHive.Users.Domain.Abstractions.Repositories
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