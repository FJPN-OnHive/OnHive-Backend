using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.Admin.Services
{
    public interface IServiceBase<T>
    {
        Task<List<T>> GetAll(string token);

        Task<PaginatedResult<T>> GetPaginated(RequestFilter filter, string token);

        Task<PaginatedResult<T>> GetByIdsPaginated(List<string> ids, RequestFilter filter, string token);

        Task<T?> GetById(string id, string token);

        Task<T?> Save(T dto, bool create, string token);

        Task<bool> Delete(string id, string token);
    }
}