using AutoMapper;
using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Models;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;
using System.Text.Json;
using System.Xml.Schema;

namespace OnHive.Orders.Services
{
    public class CartsService : ICartsService
    {
        private readonly ICartsRepository cartsRepository;
        private readonly OrdersApiSettings ordersApiSettings;
        private readonly IProductsService productsService;
        private readonly IEventRegister eventRegister;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public CartsService(ICartsRepository cartsRepository,
                            OrdersApiSettings ordersApiSettings,
                            IMapper mapper,
                            IEventRegister eventRegister,
                            IServicesHub servicesHub)
        {
            this.cartsRepository = cartsRepository;
            this.ordersApiSettings = ordersApiSettings;
            this.mapper = mapper;
            this.productsService = servicesHub.ProductsService;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<CartDto?> GetByIdAsync(string cartId)
        {
            var cart = await cartsRepository.GetByIdAsync(cartId);
            return mapper.Map<CartDto>(cart);
        }

        public async Task<PaginatedResult<CartDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await cartsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<CartDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<CartDto>>(result.Itens)
                };
            }
            return new PaginatedResult<CartDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<CartDto>()
            };
        }

        public async Task<IEnumerable<CartDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var carts = await cartsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<CartDto>>(carts);
        }

        public async Task<CartDto> SaveAsync(CartDto cartDto, LoggedUserDto? loggedUser)
        {
            var cart = mapper.Map<Cart>(cartDto);
            ValidatePermissions(cart, loggedUser?.User);
            cart.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            cart.CreatedAt = DateTime.UtcNow;
            cart.CreatedBy = string.IsNullOrEmpty(cart.CreatedBy) ? loggedUser.User.Id : cart.CreatedBy;
            var response = await cartsRepository.SaveAsync(cart);
            return mapper.Map<CartDto>(response);
        }

        public async Task<CartDto> CreateAsync(CartDto cartDto, LoggedUserDto loggedUser)
        {
            if (!cartDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var cart = mapper.Map<Cart>(cartDto);
            ValidatePermissions(cart, loggedUser.User);
            cart.Id = string.Empty;
            cart.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException("TenantId");
            var response = await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            return mapper.Map<CartDto>(response);
        }

        public async Task<CartDto?> UpdateAsync(CartDto cartDto, LoggedUserDto loggedUser)
        {
            if (!cartDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var cart = mapper.Map<Cart>(cartDto);
            ValidatePermissions(cart, loggedUser.User);
            var currentCart = await cartsRepository.GetByIdAsync(cart.Id);
            if (currentCart == null || currentCart.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            return mapper.Map<CartDto>(response);
        }

        public async Task<CartDto?> UpdateAsync(JsonDocument patch, LoggedUserDto loggedUser)
        {
            var currentCart = await cartsRepository.GetByIdAsync(patch.GetId());
            if (currentCart == null || currentCart.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            currentCart = patch.PatchEntity(currentCart);
            ValidatePermissions(currentCart, loggedUser.User);
            if (!mapper.Map<CartDto?>(currentCart).Validate(out var validationResult))
            {
                throw new InvalidPayloadException(validationResult);
            }
            var response = await cartsRepository.SaveAsync(currentCart, loggedUser.User.Id);
            return mapper.Map<CartDto>(response);
        }

        public async Task<IEnumerable<CartDto>> GetByUser(LoggedUserDto? loggedUser)
        {
            if (loggedUser?.User == null) throw new ArgumentException(nameof(loggedUser));
            var carts = await cartsRepository.GetByUserIdAsync(loggedUser.User.Id, loggedUser.User.TenantId);
            return mapper.Map<List<CartDto>>(carts);
        }

        public async Task<CartDto?> AddProduct(string cartId, string sku, LoggedUserDto loggedUser)
        {
            if (loggedUser.User == null) throw new ArgumentException(nameof(loggedUser));
            var cart = await cartsRepository.GetByIdAsync(cartId);
            if (cart == null) throw new NotFoundException("Cart not found");
            ProductDto? product = await GetProduct(sku, loggedUser);
            if (product == null) throw new ArgumentException(nameof(sku));

            if (cart.Itens.Any(i => i.ProductId == product.Id))
            {
                cart.Itens.First(i => i.ProductId == product.Id).Quantity++;
            }
            else
            {
                cart.Itens.Add(new CartItem
                {
                    Sequence = cart.Itens.Count - 1,
                    ProductId = product.Id,
                    ProductName = GetPrice(product).Item2,
                    ProductType = product.ItemType,
                    ProductValue = GetPrice(product).Item1,
                    Quantity = 1
                });
            }
            await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            return mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto?> RemoveProduct(string cartId, int sequence, LoggedUserDto loggedUser)
        {
            if (loggedUser.User == null) throw new ArgumentException(nameof(loggedUser));
            var cart = await cartsRepository.GetByIdAsync(cartId);
            if (cart == null) throw new NotFoundException("Cart not found");
            if (sequence < 0 || sequence > cart.Itens.Count - 1) throw new ArgumentException(nameof(sequence));
            if (cart.Itens.FirstOrDefault(i => i.Sequence == sequence) != null)
            {
                cart.Itens.Remove(cart.Itens.First(i => i.Sequence == sequence));
                var index = 0;
                foreach (var item in cart.Itens)
                {
                    item.Sequence = index;
                    index++;
                }
            }
            if (cart.Itens.Any())
            {
                await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            }
            else
            {
                await cartsRepository.DeleteAsync(cartId);
                cart.Id = string.Empty;
            }
            return mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto?> ChangeProductQuantity(string cartId, int sequence, int quantity, LoggedUserDto loggedUser)
        {
            if (quantity < 0) quantity = 0;
            if (quantity == 0)
            {
                return await RemoveProduct(cartId, sequence, loggedUser);
            }
            if (loggedUser.User == null) throw new ArgumentException(nameof(loggedUser));
            var cart = await cartsRepository.GetByIdAsync(cartId);
            if (cart == null) throw new NotFoundException("Cart not found");
            if (sequence < 0 || sequence > cart.Itens.Count - 1) throw new ArgumentException(nameof(sequence));
            if (cart.Itens.FirstOrDefault(i => i.Sequence == sequence) != null)
            {
                cart.Itens.First(i => i.Sequence == sequence).Quantity = quantity;
            }
            await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            return mapper.Map<CartDto>(cart);
        }

        public async Task Delete(string cartId, LoggedUserDto loggedUser)
        {
            await cartsRepository.DeleteAsync(cartId);
        }

        public async Task<CartDto?> Initialize(string sku, LoggedUserDto loggedUser)
        {
            if (loggedUser.User == null) throw new ArgumentException(nameof(loggedUser));
            ProductDto? product = await GetProduct(sku, loggedUser);
            if (product == null) throw new ArgumentException(nameof(sku));
            var cart = new Cart
            {
                UserId = loggedUser.User.Id,
                TenantId = loggedUser.User.TenantId,
                Code = CodeHelper.GenerateAlphanumericCode(16),
                PlacementDate = DateTime.UtcNow,
                Itens = new List<CartItem> { new CartItem
                    {
                        Sequence = 0,
                        ProductId = product.Id,
                        ExternalId = product.ExternalId,
                        ProductName = product.Name,
                        ProductType = product.ItemType,
                        ProductValue = GetPrice(product).Item1,
                        PriceReason = GetPrice(product).Item2,
                        Quantity = 1
                    }
                }
            };
            var result = await cartsRepository.SaveAsync(cart, loggedUser.User.Id);
            return mapper.Map<CartDto>(result);
        }

        private async Task<ProductDto?> GetProduct(string sku, LoggedUserDto loggedUser)
        {
            return await productsService.GetBySkuAsync(sku, loggedUser) ?? throw new ProductNotFoundException(sku);
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

        private void ValidatePermissions(Cart cart, UserDto? loggedUser)
        {
            if (loggedUser != null
                && !loggedUser.Permissions.Exists(p => p.Equals(ordersApiSettings.OrdersAdminPermission, StringComparison.InvariantCultureIgnoreCase))
                && loggedUser.Id != cart.UserId)
            {
                logger.Warning("Unauthorized update: {id}, logged user: {loggedUserId}", cart, loggedUser.Id);
                throw new UnauthorizedAccessException();
            }

            if (loggedUser != null && cart.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Cart/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    cart.Id, cart.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }

        private void Register(Cart cart, UserDto client, string key, string message)
        {
            _ = eventRegister.RegisterEvent(cart.TenantId, cart.UserId, key, message, new Dictionary<string, string>
            {
                { "CartId", cart.Id },
                { "ClientId", cart.UserId },
                { "ClientName", client.Name },
                { "ClientEmail", client.MainEmail },
                { "Value", cart.Itens.Sum(i => i.Quantity * i.ProductValue).ToString() },
                { "ItemName", string.Join(",", cart.Itens.Select(i => i.ProductName)) },
                { "ItemId", string.Join(",", cart.Itens.Select(i => i.ProductId)) },
                { "ItemCode", string.Join(",", cart.Itens.Select(i => i.ExternalId)) }
            });
        }
    }
}