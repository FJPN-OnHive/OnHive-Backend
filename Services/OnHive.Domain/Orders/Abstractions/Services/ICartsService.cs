using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Orders;
using System.Text.Json;

namespace EHive.Orders.Domain.Abstractions.Services
{
    public interface ICartsService
    {
        Task<CartDto?> GetByIdAsync(string cartId);

        Task<PaginatedResult<CartDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<CartDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<CartDto> SaveAsync(CartDto cartDto, LoggedUserDto? user);

        Task<CartDto> CreateAsync(CartDto cartDto, LoggedUserDto loggedUser);

        Task<CartDto?> UpdateAsync(CartDto cartDto, LoggedUserDto loggedUser);

        Task<CartDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<IEnumerable<CartDto>> GetByUser(LoggedUserDto? loggedUser);

        Task<CartDto?> Initialize(string sku, LoggedUserDto loggedUser);

        Task<CartDto?> AddProduct(string cartId, string sku, LoggedUserDto loggedUser);

        Task<CartDto?> RemoveProduct(string cartId, int sequence, LoggedUserDto loggedUser);

        Task<CartDto?> ChangeProductQuantity(string cartId, int sequence, int quantity, LoggedUserDto loggedUser);

        Task Delete(string cartId, LoggedUserDto loggedUser);
    }
}