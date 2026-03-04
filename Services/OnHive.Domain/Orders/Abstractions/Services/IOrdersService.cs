using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Enums.Orders;
using System.Text.Json;

namespace OnHive.Orders.Domain.Abstractions.Services
{
    public interface IOrdersService
    {
        Task<OrderDto?> GetByIdAsync(string orderId, LoggedUserDto? loggedUser);

        Task<PaginatedResult<OrderDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser);

        Task<IEnumerable<OrderDto>> GetAllAsync(LoggedUserDto? loggedUser);

        Task<OrderDto> SaveAsync(OrderDto orderDto, LoggedUserDto? user);

        Task<OrderDto> CreateAsync(OrderDto orderDto, LoggedUserDto loggedUser);

        Task<OrderDto?> UpdateAsync(OrderDto orderDto, LoggedUserDto loggedUser);

        Task<OrderDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser);

        Task<OrderDto?> Initialize(string sku, LoggedUserDto loggedUser);

        Task<OrderDto?> FromCart(string cartId, LoggedUserDto loggedUser);

        Task<OrderDto?> Cancel(string orderId, LoggedUserDto loggedUser);

        Task<OrderDto?> RequestRefound(string orderId, LoggedUserDto loggedUser);

        Task SetPaymentStatus(string orderId, string paymentId, OrderStatus status, LoggedUserDto loggedUser);
    }
}