using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Emails;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Core.Library.Enums.Orders;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Models;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Models;
using OnHive.Payments.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Services;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;
using System.Text.Json;

namespace OnHive.Orders.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly ICartsRepository cartsRepository;
        private readonly IEventRegister eventRegister;
        private readonly IStudentsService studentsService;
        private readonly IInvoicesService invoicesService;
        private readonly IEmailsService emailsService;
        private readonly IPaymentsService paymentsService;
        private readonly IProductsService productsService;
        private readonly OrdersApiSettings ordersApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public OrdersService(IOrdersRepository ordersRepository,
                             OrdersApiSettings ordersApiSettings,
                             ICartsRepository cartsRepository,
                             IMapper mapper,
                             IEventRegister eventRegister,
                             IServicesHub servicesHub)
        {
            this.ordersRepository = ordersRepository;
            this.cartsRepository = cartsRepository;
            this.ordersApiSettings = ordersApiSettings;
            this.mapper = mapper;
            this.eventRegister = eventRegister;
            this.studentsService = servicesHub.StudentsService;
            this.emailsService = servicesHub.EmailsService;
            this.invoicesService = servicesHub.InvoicesService;
            this.productsService = servicesHub.ProductsService;
            this.paymentsService = servicesHub.PaymentsService;
            logger = Log.Logger;
        }

        public async Task<OrderDto?> GetByIdAsync(string orderId, LoggedUserDto? loggedUser)
        {
            var order = await ordersRepository.GetByIdAsync(orderId);
            if (loggedUser != null)
            {
                Register(order, loggedUser.User, EventKeys.OrderView, "Order loaded");
            }
            return mapper.Map<OrderDto>(order);
        }

        public async Task<PaginatedResult<OrderDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await ordersRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<OrderDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<OrderDto>>(result.Itens)
                };
            }
            return new PaginatedResult<OrderDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<OrderDto>()
            };
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var orders = await ordersRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> SaveAsync(OrderDto orderDto, LoggedUserDto? loggedUser)
        {
            var order = mapper.Map<Order>(orderDto);
            ValidatePermissions(order, loggedUser?.User);
            order.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            order.CreatedAt = DateTime.UtcNow;
            order.CreatedBy = string.IsNullOrEmpty(order.CreatedBy) ? loggedUser.User.Id : order.CreatedBy;
            var response = await ordersRepository.SaveAsync(order);
            return mapper.Map<OrderDto>(response);
        }

        public async Task<OrderDto> CreateAsync(OrderDto orderDto, LoggedUserDto loggedUser)
        {
            if (!orderDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var order = mapper.Map<Order>(orderDto);
            ValidatePermissions(order, loggedUser.User);
            order.Id = string.Empty;
            order.TenantId = loggedUser.User?.TenantId ?? throw new ArgumentException("TenantId");
            var response = await ordersRepository.SaveAsync(order, loggedUser.User.Id);
            await SendEmail(order, loggedUser, ordersApiSettings.OrderCreatedTemplate!);
            Register(order, loggedUser.User, EventKeys.OrderCreated, "Order created");
            return mapper.Map<OrderDto>(response);
        }

        public async Task<OrderDto?> UpdateAsync(OrderDto orderDto, LoggedUserDto loggedUser)
        {
            if (!orderDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var order = mapper.Map<Order>(orderDto);
            ValidatePermissions(order, loggedUser.User);
            var currentOrder = await ordersRepository.GetByIdAsync(order.Id);
            if (currentOrder == null || currentOrder.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await ordersRepository.SaveAsync(order, loggedUser.User.Id);
            return mapper.Map<OrderDto>(response);
        }

        public async Task<OrderDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentOrder = await ordersRepository.GetByIdAsync(patch.GetId());
            if (currentOrder == null || currentOrder.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentOrder = patch.PatchEntity(currentOrder);
            ValidatePermissions(currentOrder, loggedUser.User);
            if (!mapper.Map<OrderDto?>(currentOrder).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await ordersRepository.SaveAsync(currentOrder, loggedUser.User.Id);
            return mapper.Map<OrderDto>(response);
        }

        public async Task<OrderDto?> FromCart(string cartId, LoggedUserDto loggedUser)
        {
            var cart = await cartsRepository.GetByIdAsync(cartId) ?? throw new NotFoundException($"Cart: {cartId}");
            var order = new Order
            {
                UserId = loggedUser?.User?.Id ?? throw new ArgumentException(nameof(loggedUser)),
                ExternalId = cart.ExternalId,
                Code = cart.Code,
                Status = OrderStatus.Pending,
                PlacementDate = DateTime.UtcNow
            };
            order.Itens = cart.Itens.Select(i => new OrderItem
            {
                Sequence = i.Sequence,
                ExternalId = i.ExternalId,
                Quantity = i.Quantity,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductDescription = i.ProductDescription,
                ProductType = i.ProductType,
                Price = i.ProductValue,
                TotalPrice = i.ProductValue * i.Quantity,
                PriceReason = i.PriceReason
            }).ToList();
            order.TotalValue = order.Itens.Sum(i => i.TotalPrice);
            var result = await ordersRepository.SaveAsync(order, loggedUser.User.Id);
            await cartsRepository.DeleteAsync(cartId);
            await SendEmail(order, loggedUser, ordersApiSettings.OrderCreatedTemplate!);
            Register(order, loggedUser.User, EventKeys.OrderCreated, "Order created from cart");
            return mapper.Map<OrderDto>(result);
        }

        public async Task<OrderDto?> Cancel(string orderId, LoggedUserDto loggedUser)
        {
            var order = await ordersRepository.GetByIdAsync(orderId) ?? throw new NotFoundException($"Order: {orderId}");
            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.PaymentRefused)
            {
                order.Status = OrderStatus.Cancelled;
                await ordersRepository.SaveAsync(order, loggedUser?.User?.Id);
            }
            else
            {
                throw new InvalidStatusException($"Invalid status for cancellation: {order.Status} ");
            }
            Register(order, loggedUser.User, EventKeys.OrderAbandoned, "Order Deleted");
            return mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto?> RequestRefound(string orderId, LoggedUserDto loggedUser)
        {
            var order = await ordersRepository.GetByIdAsync(orderId) ?? throw new NotFoundException($"Order: {orderId}");
            if (order.Status == OrderStatus.Closed)
            {
                await PaymentRefound(order, loggedUser);
                order.Status = OrderStatus.RefoundRequsted;
                await ordersRepository.SaveAsync(order, loggedUser?.User?.Id);
            }
            else
            {
                throw new InvalidStatusException($"Invalid status/date for refound: {order.Status} / {order.ClosingDate} ");
            }
            return mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto?> Initialize(string sku, LoggedUserDto loggedUser)
        {
            if (loggedUser.User == null) throw new ArgumentException(nameof(loggedUser));
            ProductDto? product = await GetProduct(sku, loggedUser);
            if (product == null) throw new ArgumentException(nameof(sku));
            var order = new Order
            {
                UserId = loggedUser.User.Id,
                TenantId = loggedUser.User.TenantId,
                Code = CodeHelper.GenerateAlphanumericCode(16),
                PlacementDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Itens = new List<OrderItem> { new OrderItem
                    {
                        Sequence = 0,
                        ProductId = product.Id,
                        ProductDescription = product.Description,
                        ProductName = product.Name,
                        ExternalId = product.ExternalId,
                        ProductType = product.ItemType,
                        Price = GetPrice(product).Item1,
                        PriceReason = GetPrice(product).Item2,
                        TotalPrice = GetPrice(product).Item1,
                        Quantity = 1
                    }
                }
            };
            order.TotalValue = order.Itens.Sum(i => i.Price * i.Quantity);
            var result = await ordersRepository.SaveAsync(order, loggedUser.User.Id);
            await SendEmail(order, loggedUser, ordersApiSettings.OrderCreatedTemplate!);
            Register(order, loggedUser.User, EventKeys.OrderCreated, "Order initialized");
            return mapper.Map<OrderDto>(result);
        }

        private async Task PaymentRefound(Order order, LoggedUserDto loggedUser)
        {
            var recipeResponse = await paymentsService.GetReceiptByOrder(order.Id, loggedUser);
            if (recipeResponse == null)
            {
                throw new NotFoundException(order.PaymentId);
            }
            if (recipeResponse?.Status != Core.Library.Enums.Payments.PaymentStatus.Confirmed)
            {
                throw new InvalidStatusException($"Invalid payment status: {recipeResponse?.Status}");
            }
            var refoundResponse = await paymentsService.Cancel(recipeResponse?.PaymentId, loggedUser);
            if (refoundResponse == null)
            {
                throw new InvalidOperationException($"Error refounding: {recipeResponse?.PaymentId}");
            }
        }

        private (double, string) GetPrice(ProductDto product)
        {
            var result = (product.FullPrice, "BasePrice");
            if (product.Prices != null && product.Prices.Any())
            {
                var price = product.Prices.FirstOrDefault(p => p.EndDate >= DateTime.UtcNow, new ProductPriceDto { Price = product.FullPrice, Description = "BasePrice" });
                result = (price.Price, price.Description);
            }
            return result;
        }

        private async Task<ProductDto?> GetProduct(string sku, LoggedUserDto loggedUser)
        {
            return await productsService.GetBySkuAsync(sku, loggedUser) ?? throw new ProductNotFoundException($"{sku}");
        }

        private void ValidatePermissions(Order order, UserDto? loggedUser)
        {
            if (loggedUser != null
                && !loggedUser.Permissions.Exists(p => p.Equals(ordersApiSettings.OrdersAdminPermission, StringComparison.InvariantCultureIgnoreCase))
                && loggedUser.Id != order.UserId)
            {
                logger.Warning("Unauthorized update: {id}, logged user: {loggedUserId}", order, loggedUser?.Id);
                throw new UnauthorizedAccessException();
            }

            if (loggedUser != null && order.TenantId != loggedUser?.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Order/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    order.Id, order.TenantId, loggedUser?.Id, loggedUser?.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        public async Task SetPaymentStatus(string orderId, string paymentId, OrderStatus status, LoggedUserDto loggedUser)
        {
            var order = await ordersRepository.GetByIdAsync(orderId) ?? throw new NotFoundException($"Order: {orderId}");
            order.Status = status;
            order.PaymentId = paymentId;
            switch (status)
            {
                case OrderStatus.Closed:
                    order.ClosingDate = DateTime.Now;
                    await SendEmail(order, loggedUser, ordersApiSettings.OrderCheckoutTemplate!);
                    await RequestInvoice(order);
                    await RequestEnrollment(order);
                    break;

                case OrderStatus.Cancelled:
                    order.CancellationDate = DateTime.Now;
                    break;

                case OrderStatus.Refounded:
                    order.RefoundDate = DateTime.UtcNow;
                    break;
            }
            await ordersRepository.SaveAsync(order);
        }

        private async Task RequestEnrollment(Order order)
        {
            try
            {
                var messages = order.Itens
                    .Select(i => new EnrollmentMessage
                    {
                        OrderId = order.Id,
                        UserId = order.UserId,
                        TenantId = order.TenantId,
                        ProductId = i.ProductId
                    })
                    .ToList();

                foreach (var message in messages)
                {
                    logger.Information("Requesting enrollment: {message}", JsonSerializer.Serialize(message));
                    _ = studentsService.InternalEnroll(message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Send order enrollment request failed: {message}", ex.Message, ex);
            }
        }

        private async Task RequestInvoice(Order order)
        {
            try
            {
                var message = new InvoiceMessage
                {
                    TenantId = order.TenantId,
                    OrderId = order.Id,
                    ProviderKey = ordersApiSettings.InvoiceProvider
                };
                _ = invoicesService.InitializeInvoice(message);
            }
            catch (Exception ex)
            {
                logger.Error("Send order invoice request failed: {message}", ex.Message, ex);
            }
        }

        private async Task SendEmail(Order order, LoggedUserDto loggedUser, string template)
        {
            try
            {
                var emailValidation = new EmailSendDto
                {
                    TenantId = order.TenantId,
                    SendTo = new List<string> { loggedUser?.User?.MainEmail ?? throw new ArgumentException("User email") },
                    Fields = new Dictionary<string, string> {
                    { "ORDER", order.Code },
                    { "NAME", loggedUser.User.Name },
                    { "LINK", $"{ordersApiSettings.OrderViewUrl}?tenantId={order.TenantId}&email={loggedUser?.User?.MainEmail}&user={loggedUser?.User?.Id}&order={order.Code}" },
                    { "ITENS", string.Join("\n", order.Itens.Select(i => $"{i.ProductName} - {i.TotalPrice}")) }
                },
                    TemplateCode = template
                };
                _ = emailsService.ComposeEmail(emailValidation);
            }
            catch (Exception ex)
            {
                logger.Error("Send order email failed: {message}", ex.Message, ex);
            }
        }

        private void Register(Order order, UserDto client, string key, string message)
        {
            _ = eventRegister.RegisterEvent(order.TenantId, order.UserId, key, message, new Dictionary<string, string>
            {
                { "OrderId", order.Id },
                { "ClientId", order.UserId },
                { "ClientName", client.Name },
                { "ClientEmail", client.MainEmail },
                { "Value", order.TotalValue.ToString() },
                { "ItemName", string.Join(",", order.Itens.Select(i => i.ProductName)) },
                { "ItemId", string.Join(",", order.Itens.Select(i => i.ProductId)) },
                { "ItemCode", string.Join(",", order.Itens.Select(i => i.ExternalId)) }
            });
        }
    }
}