using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Invoices;
using OnHive.Core.Library.Enums.Common;
using OnHive.Core.Library.Enums.Invoices;
using OnHive.Core.Library.Helpers;
using OnHive.Core.Library.Validations.Common;
using OnHive.Invoices.Domain.Abstractions.Repositories;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Models;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Domains.Common.Abstractions.Services;
using Serilog;
using System.Text;
using System.Text.Json;

namespace OnHive.Invoices.Services
{
    public class InvoicesService : IInvoicesService
    {
        private readonly IInvoicesRepository invoicesRepository;
        private readonly IUsersService usersService;
        private readonly IOrdersService ordersService;
        private readonly ITenantsService tenantsService;
        private readonly ITenantParametersService tenantParametersService;
        private readonly InvoicesApiSettings invoicesApiSettings;
        private readonly HttpClient httpClient;

        private readonly IMapper mapper;
        private readonly ILogger logger;

        public InvoicesService(IInvoicesRepository invoicesRepository,
                               InvoicesApiSettings invoicesApiSettings,
                               IMapper mapper,
                               HttpClient httpClient,
                               IServicesHub servicesHub)
        {
            this.usersService = servicesHub.UsersService;
            this.ordersService = servicesHub.OrdersService;
            this.tenantsService = servicesHub.TenantsService;
            this.tenantParametersService = servicesHub.TenantParametersService;
            this.invoicesRepository = invoicesRepository;
            this.invoicesApiSettings = invoicesApiSettings;
            this.httpClient = httpClient;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<InvoiceDto?> InitializeInvoice(InvoiceMessage invoiceMessage)
        {
            var provider = GetProvider(invoiceMessage);
            var order = await GetOrder(invoiceMessage);
            var emitter = await GetEmitter(order);
            var invoiceSerie = await GetSerie(order.TenantId);
            var client = await GetClient(order);
            var invoice = await CreateInvoice(order, emitter, client, invoiceSerie, invoiceMessage.ProviderKey);
            invoice = await invoicesRepository.SaveAsync(invoice);
            invoice = await SendInvoice(provider, invoice);
            invoice = await invoicesRepository.SaveAsync(invoice);
            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task VerifyPendingInvoices()
        {
            var pendingInvoices = await invoicesRepository.GetPendingInvoices();

            foreach (var invoice in pendingInvoices)
            {
                try
                {
                    var provider = GetProvider(new InvoiceMessage { ProviderKey = invoice.Provider });
                    var resultInvoice = await VerifyInvoice(provider, invoice);
                    await invoicesRepository.SaveAsync(resultInvoice);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error verifying invoice: {id}", invoice.Id);
                }
            }
        }

        public async Task<InvoiceDto?> GetByIdAsync(string invoiceId)
        {
            var invoice = await invoicesRepository.GetByIdAsync(invoiceId);
            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<PaginatedResult<InvoiceDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await invoicesRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId);
            if (result != null)
            {
                return new PaginatedResult<InvoiceDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Itens = mapper.Map<List<InvoiceDto>>(result.Itens)
                };
            }
            return new PaginatedResult<InvoiceDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<InvoiceDto>()
            };
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var invoices = await invoicesRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDto> SaveAsync(InvoiceDto invoiceDto, LoggedUserDto? loggedUser)
        {
            var invoice = mapper.Map<Invoice>(invoiceDto);
            ValidatePermissions(invoice, loggedUser?.User);
            invoice.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            invoice.CreatedAt = DateTime.UtcNow;
            invoice.CreatedBy = string.IsNullOrEmpty(invoice.CreatedBy) ? loggedUser?.User?.Id : invoice.CreatedBy;

            var response = await invoicesRepository.SaveAsync(invoice);
            return mapper.Map<InvoiceDto>(response);
        }

        public async Task<InvoiceDto> CreateAsync(InvoiceDto invoiceDto, LoggedUserDto? loggedUser)
        {
            if (!invoiceDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var invoice = mapper.Map<Invoice>(invoiceDto);
            ValidatePermissions(invoice, loggedUser?.User);
            invoice.Id = string.Empty;
            invoice.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await invoicesRepository.SaveAsync(invoice, loggedUser.User.Id);
            return mapper.Map<InvoiceDto>(response);
        }

        public async Task<InvoiceDto?> UpdateAsync(InvoiceDto invoiceDto, LoggedUserDto? loggedUser)
        {
            if (!invoiceDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var invoice = mapper.Map<Invoice>(invoiceDto);
            ValidatePermissions(invoice, loggedUser?.User);
            var currentInvoice = await invoicesRepository.GetByIdAsync(invoice.Id);
            if (currentInvoice == null || currentInvoice.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var response = await invoicesRepository.SaveAsync(invoice, loggedUser.User.Id);
            return mapper.Map<InvoiceDto>(response);
        }

        public async Task<InvoiceDto?> UpdateAsync(JsonDocument patch, LoggedUserDto? loggedUser)
        {
            var currentInvoice = await invoicesRepository.GetByIdAsync(patch.GetId());
            if (currentInvoice == null || currentInvoice.TenantId != loggedUser.User.TenantId)
            {
                return null;
            }
            currentInvoice = patch.PatchEntity(currentInvoice);
            ValidatePermissions(currentInvoice, loggedUser.User);
            var response = await invoicesRepository.SaveAsync(currentInvoice, loggedUser.User.Id);
            return mapper.Map<InvoiceDto>(response);
        }

        private async Task<Invoice> SendInvoice(InvoiceProvider provider, Invoice? invoice)
        {
            var invoiceDto = mapper.Map<InvoiceDto>(invoice);
            var content = new StringContent(JsonSerializer.Serialize(invoiceDto), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{provider.Url}/Internal/Emission", content);
            response.EnsureSuccessStatusCode();
            var invoiceResponse = JsonSerializer.Deserialize<Response<InvoiceDto>>(await response.Content.ReadAsStringAsync());
            if (invoiceResponse?.Code != ResponseCode.OK) throw new ArgumentException("Invoice Emission not found");
            invoiceDto = invoiceResponse.Payload ?? throw new ArgumentException("Invoice Emission not found");
            invoice = mapper.Map<Invoice>(invoiceDto);
            return invoice;
        }

        private async Task<Invoice> VerifyInvoice(InvoiceProvider provider, Invoice? invoice)
        {
            var invoiceDto = mapper.Map<InvoiceDto>(invoice);
            var content = new StringContent(JsonSerializer.Serialize(invoiceDto), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{provider.Url}/Internal/Verify", content);
            response.EnsureSuccessStatusCode();
            var invoiceResponse = JsonSerializer.Deserialize<Response<InvoiceDto>>(await response.Content.ReadAsStringAsync());
            if (invoiceResponse?.Code != ResponseCode.OK) throw new ArgumentException("Invoice Emission not found");
            invoiceDto = invoiceResponse.Payload ?? throw new ArgumentException("Invoice Emission not found");
            invoice = mapper.Map<Invoice>(invoiceDto);
            return invoice;
        }

        private async Task<string> GetSerie(string tenantId)
        {
            var parameter = await tenantParametersService.GetByKey("INVOICE", "SERIE", tenantId);
            if (parameter != null)
            {
                return parameter.Value;
            }
            else
            {
                return "1";
            }
        }

        private async Task<Invoice> CreateInvoice(OrderDto order, TenantDto emitter, UserDto client, string invoiceSerie, string providerKey)
        {
            var mainAddress = client.Addresses.Find(a => a.IsMainAddress) ?? client.Addresses.FirstOrDefault() ?? throw new ArgumentException("Client address not found.");
            var lastInvoicenumber = await invoicesRepository.GetLastInvoiceNumber(order.TenantId, invoiceSerie);
            return new Invoice
            {
                Emitter = new InvoiceEmitter
                {
                    Name = emitter.Name,
                    StateInscription = emitter.StateInscription,
                    CityInscription = emitter.CityInscription,
                    CNPJ = emitter.CNPJ,
                    Address = emitter.Address,
                    City = emitter.City,
                    State = emitter.State,
                    PostalCode = emitter.PostalCode,
                    Country = emitter.Country,
                    District = emitter.District
                },
                Client = new InvoiceClient
                {
                    Name = client.Name,
                    Document = client.Documents.Find(d => d.DocumentType == "CPF")?.DocumentNumber ?? client.Documents.FirstOrDefault()?.DocumentNumber ?? string.Empty,
                    DocumentType = "CPF",
                    Address = new Core.Library.Entities.Users.Address
                    {
                        AddressLines = mainAddress.AddressLines,
                        City = mainAddress.City,
                        Complement = mainAddress.Complement,
                        Country = mainAddress.Country.Name,
                        District = mainAddress.District,
                        Name = mainAddress.Name,
                        Number = mainAddress.Number,
                        State = mainAddress.State.Code,
                        ZipCode = mainAddress.ZipCode
                    },
                },
                EmissionDate = DateTime.UtcNow,
                Itens = order.Itens.Select(i => new InvoiceItens
                {
                    Description = i.ProductDescription,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitValue = i.Price,
                    TotalValue = i.TotalPrice
                }).ToList(),
                TotalValue = order.Itens.Sum(i => i.TotalPrice),
                UserId = order.UserId,
                OrderId = order.Id,
                Status = InvoiceStatus.Pending,
                Series = invoiceSerie,
                Number = lastInvoicenumber + 1,
                TenantId = order.TenantId,
                PaymentId = order.PaymentId,
                Discount = order.OrderDiscount,
                Provider = providerKey,
                Token = string.Empty,
                Message = string.Empty
            };
        }

        private async Task<UserDto> GetClient(OrderDto order)
        {
            return await usersService.GetByIdAsync(order.UserId) ?? throw new ArgumentException("Client not found");
        }

        private async Task<TenantDto> GetEmitter(OrderDto order)
        {
            return await tenantsService.GetByIdAsync(order.TenantId) ?? throw new ArgumentException("Emitter not found");
        }

        private async Task<OrderDto> GetOrder(InvoiceMessage invoiceMessage)
        {
            return await ordersService.GetByIdAsync(invoiceMessage.OrderId, null) ?? throw new ArgumentException("Order not found");
        }

        private InvoiceProvider GetProvider(InvoiceMessage invoiceMessage)
        {
            var provider = invoicesApiSettings?.InvoiceProviders?.Find(p => p.Name.Equals(invoiceMessage.ProviderKey, StringComparison.InvariantCultureIgnoreCase));
            provider ??= invoicesApiSettings?.InvoiceProviders?.Find(p => p.IsDefault);
            if (provider == null) throw new ArgumentException("Provider not found");
            return provider;
        }

        private void ValidatePermissions(Invoice invoice, UserDto? loggedUser)
        {
            if (loggedUser != null && invoice.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Invoice/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    invoice.Id, invoice.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}