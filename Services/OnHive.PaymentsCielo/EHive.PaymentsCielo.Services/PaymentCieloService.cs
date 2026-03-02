using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Enums.Payments;
using EHive.PaymentsCielo.Domain.Abstractions.Services;
using EHive.PaymentsCielo.Domain.Contracts;
using EHive.PaymentsCielo.Domain.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EHive.PaymentsCielo.Services
{
    public class PaymentCieloService : IPaymentCieloService
    {
        private readonly PaymentCieloSettings settings;
        private readonly HttpClient httpClient;

        public PaymentCieloService(PaymentCieloSettings settings, HttpClient httpClient)
        {
            this.settings = settings;
            this.httpClient = httpClient;
        }

        public async Task<PaymentReceiptDto> CheckoutAsync(PaymentCheckoutDto paymentCheckout)
        {
            return paymentCheckout.PaymentType switch
            {
                PaymentType.BankSlip => await ProcessBankSlipAsync(paymentCheckout),
                PaymentType.CreditCard => await ProcessCreditCardAsync(paymentCheckout),
                PaymentType.DebitCard => await ProcessDebitCardAsync(paymentCheckout),
                PaymentType.PIX => await ProcessPixAsync(paymentCheckout),
                _ => throw new ArgumentException("Invalid Payment Type"),
            };
        }

        public async Task<PaymentReceiptDto?> GetPaymentAsync(PaymentReceiptDto paymentReceipt)
        {
            switch (paymentReceipt.Type)
            {
                case PaymentType.BankSlip:
                    var resultBankSlip = await SendQuery<CieloBankSlipResponse>(paymentReceipt.ExternalId);
                    return GetReceiptBankSlip(paymentReceipt, resultBankSlip);

                case PaymentType.CreditCard:
                    var resultCreditCard = await SendQuery<CieloCreditCardResponse>(paymentReceipt.ExternalId);
                    return GetReceiptCreditCard(paymentReceipt, resultCreditCard);

                case PaymentType.DebitCard:
                    var resultDebitCard = await SendQuery<CieloDebitCardResponse>(paymentReceipt.ExternalId);
                    return GetReceiptDebitCard(paymentReceipt, resultDebitCard);

                case PaymentType.PIX:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Invalid Payment Type");
            }
        }

        public async Task<PaymentReceiptDto?> CancelPaymentAsync(PaymentReceiptDto paymentReceipt)
        {
            var result = paymentReceipt.Type switch
            {
                PaymentType.BankSlip => await SendCancel(paymentReceipt.ExternalId),
                PaymentType.CreditCard => await SendCancel(paymentReceipt.ExternalId),
                PaymentType.DebitCard => await SendCancel(paymentReceipt.ExternalId),
                PaymentType.PIX => throw new NotImplementedException(),
                _ => throw new ArgumentException("Invalid Payment Type"),
            };
            return GetReceiptCancel(paymentReceipt, result);
        }

        public Task<List<PaymentTypeDto>> GetPaymentTypesAsync()
        {
            return Task.Run(() =>
            {
                return new List<PaymentTypeDto> {
                    new PaymentTypeDto
                    {
                        Id = "1",
                        Name = "Visa Crédito",
                        PaymentType = PaymentType.CreditCard,
                        Status = PaymentTypeStatus.Active,
                        SecureThumbnail = "https://www.mercadopago.com/org-img/MP3/API/logos/visa.gif",
                        Thumbnail = "https://www.mercadopago.com/org-img/MP3/API/logos/visa.gif",
                        AccreditationTime  = 5000,
                        Settings = new Settings
                        {
                            Bin = new Bin {
                                Pattern = "^(4)",
                                ExclusionPattern= "^(400163|400176|400178|400185|400199|423808|439267|471233|473200|476332|482481|451416|438935|(40117[8-9])|(45763[1-2])|457393|431274)",
                                InstallmentsPattern = "^(?!(417401|453998|426398|462437|451212|456188))" },
                            CardNumber = new CardNumber { Length = 16, Validation = "standard"},
                            SecurityCode = new SecurityCode { Mode = "mandatory", Length = 3, CardLocation = "back"},
                        },
                        MinAllowedAmount = 0.5,
                        MaxAllowedAmount = 60000
                    },
                    new PaymentTypeDto
                    {
                        Id = "2",
                        Name = "Master Crédito",
                        PaymentType = PaymentType.CreditCard,
                        Status = PaymentTypeStatus.Active,
                        SecureThumbnail = "https://www.mercadopago.com/org-img/MP3/API/logos/master.gif",
                        Thumbnail = "https://www.mercadopago.com/org-img/MP3/API/logos/master.gif",
                        AccreditationTime  = 5000,
                        Settings = new Settings
                        {
                            Bin = new Bin {
                                Pattern = "^(4)",
                                ExclusionPattern= "^(400163|400176|400178|400185|400199|423808|439267|471233|473200|476332|482481|451416|438935|(40117[8-9])|(45763[1-2])|457393|431274)",
                                InstallmentsPattern = "^(?!(417401|453998|426398|462437|451212|456188))" },
                            CardNumber = new CardNumber { Length = 16, Validation = "standard"},
                            SecurityCode = new SecurityCode { Mode = "mandatory", Length = 3, CardLocation = "back"},
                        },
                        MinAllowedAmount = 0.5,
                        MaxAllowedAmount = 60000
                    },
                     new PaymentTypeDto
                    {
                        Id = "3",
                        Name = "Boleto",
                        PaymentType = PaymentType.BankSlip,
                        Status = PaymentTypeStatus.Active,
                        SecureThumbnail = "",
                        Thumbnail = "",
                        AccreditationTime  = 5000,
                        MinAllowedAmount = 0.5,
                        MaxAllowedAmount = 60000
                    }
                };
            });
        }

        public async Task<ProviderInfoDto> GetProviderInfoAsync()
        {
            var info = new ProviderInfoDto
            {
                Key = "CIELO",
                Name = "Cielo",
                Description = "Cielo payment provider",
                IsAsync = true,
                Fields = new List<ProviderField>(),
                IsActive = settings.IsActive
            };

            info.PaymentTypes = await GetPaymentTypesAsync();

            return info;
        }

        private async Task<PaymentReceiptDto> ProcessPixAsync(PaymentCheckoutDto paymentCheckout)
        {
            var cieloRequest = new CieloPixRequest
            {
                MerchantOrderId = paymentCheckout.OrderNumber,
                Customer = new Customer
                {
                    Name = paymentCheckout.PaymentClient.Name,
                    Email = paymentCheckout.PaymentClient.Email,
                    Identity = paymentCheckout.PaymentClient.Identification,
                    IdentityType = paymentCheckout.PaymentClient.IdentificationType,
                    Birthdate = paymentCheckout.PaymentClient.BirthDate,
                    Address = new Address
                    {
                        Street = paymentCheckout.PaymentClient.Address.AddressLines,
                        Number = paymentCheckout.PaymentClient.Address.Number,
                        Complement = paymentCheckout.PaymentClient.Address.Complement,
                        ZipCode = paymentCheckout.PaymentClient.Address.ZipCode,
                        City = paymentCheckout.PaymentClient.Address.City,
                        State = paymentCheckout.PaymentClient.Address.State.Code,
                        Country = "BRA",
                        District = paymentCheckout.PaymentClient.Address.District
                    }
                },
                Payment = new PaymentPix
                {
                    Amount = (int)(paymentCheckout.Value * 100)
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(cieloRequest, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            var response = await SendTransactional<CieloPixResponse>(content);
            return GetReceiptPix(paymentCheckout, response);
        }

        private async Task<PaymentReceiptDto> ProcessDebitCardAsync(PaymentCheckoutDto paymentCheckout)
        {
            var cieloRequest = new CieloDebitCardRequest
            {
                MerchantOrderId = paymentCheckout.OrderNumber,
                Customer = new Customer
                {
                    Name = paymentCheckout.PaymentClient.Name,
                    Email = paymentCheckout.PaymentClient.Email,
                    Identity = paymentCheckout.PaymentClient.Identification,
                    IdentityType = paymentCheckout.PaymentClient.IdentificationType,
                    Birthdate = paymentCheckout.PaymentClient.BirthDate,
                    Address = new Address
                    {
                        Street = paymentCheckout.PaymentClient.Address.AddressLines,
                        Number = paymentCheckout.PaymentClient.Address.Number,
                        Complement = paymentCheckout.PaymentClient.Address.Complement,
                        ZipCode = paymentCheckout.PaymentClient.Address.ZipCode,
                        City = paymentCheckout.PaymentClient.Address.City,
                        State = paymentCheckout.PaymentClient.Address.State.Code,
                        Country = "BRA",
                        District = paymentCheckout.PaymentClient.Address.District
                    }
                },
                Payment = new PaymentDebitCard
                {
                    Amount = (int)(paymentCheckout.Value * 100),
                    DebitCard = new DebitCard
                    {
                        CardNumber = paymentCheckout.CardInfo.CardNumber,
                        Holder = paymentCheckout.CardInfo.CardHolder.Name,
                        ExpirationDate = paymentCheckout.CardInfo.ExpirationDate,
                        SecurityCode = paymentCheckout.CardInfo.SecurityCode,
                        Brand = paymentCheckout.CardInfo.Issuer
                    },
                    ReturnUrl = paymentCheckout.ReturnUrl
                }
            };
            if (cieloRequest.Payment.DebitCard.Brand.Equals("MasterCard", StringComparison.InvariantCultureIgnoreCase))
            {
                cieloRequest.Payment.InitiatedTransactionIndicator = new InitiatedTransactionIndicator
                {
                    Category = "C1",
                    Subcategory = "Standingorder"
                };
            }

            var content = new StringContent(JsonSerializer.Serialize(cieloRequest, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            var response = await SendTransactional<CieloDebitCardResponse>(content);
            return GetReceiptDebitCard(paymentCheckout, response);
        }

        private async Task<PaymentReceiptDto> ProcessCreditCardAsync(PaymentCheckoutDto paymentCheckout)
        {
            var cieloRequest = new CieloCreditCardRequest
            {
                MerchantOrderId = paymentCheckout.OrderNumber,
                Customer = new Customer
                {
                    Name = paymentCheckout.PaymentClient.Name,
                    Email = paymentCheckout.PaymentClient.Email,
                    Identity = paymentCheckout.PaymentClient.Identification,
                    IdentityType = paymentCheckout.PaymentClient.IdentificationType,
                    Birthdate = paymentCheckout.PaymentClient.BirthDate,
                    Address = new Address
                    {
                        Street = paymentCheckout.PaymentClient.Address.AddressLines,
                        Number = paymentCheckout.PaymentClient.Address.Number,
                        Complement = paymentCheckout.PaymentClient.Address.Complement,
                        ZipCode = paymentCheckout.PaymentClient.Address.ZipCode,
                        City = paymentCheckout.PaymentClient.Address.City,
                        State = paymentCheckout.PaymentClient.Address.State.Code,
                        Country = "BRA",
                        District = paymentCheckout.PaymentClient.Address.District
                    }
                },
                Payment = new PaymentCreditCard
                {
                    Amount = (int)(paymentCheckout.Value * 100),
                    Installments = paymentCheckout.Installments > 0 ? paymentCheckout.Installments : 1,
                    Currency = "BRL",
                    Country = "BRA",
                    CreditCard = new CreditCard
                    {
                        CardNumber = paymentCheckout.CardInfo.CardNumber,
                        Holder = paymentCheckout.CardInfo.CardHolder.Name,
                        ExpirationDate = paymentCheckout.CardInfo.ExpirationDate,
                        SecurityCode = paymentCheckout.CardInfo.SecurityCode,
                        Brand = paymentCheckout.CardInfo.Issuer
                    }
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(cieloRequest, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            var response = await SendTransactional<CieloCreditCardResponse>(content);
            return GetReceiptCreditCard(paymentCheckout, response);
        }

        private async Task<PaymentReceiptDto> ProcessBankSlipAsync(PaymentCheckoutDto paymentCheckout)
        {
            var cieloRequest = new CieloBankSlipRequest
            {
                MerchantOrderId = paymentCheckout.OrderNumber,
                Customer = new Customer
                {
                    Name = paymentCheckout.PaymentClient.Name,
                    Email = paymentCheckout.PaymentClient.Email,
                    Identity = paymentCheckout.PaymentClient.Identification,
                    IdentityType = paymentCheckout.PaymentClient.IdentificationType,
                    Birthdate = paymentCheckout.PaymentClient.BirthDate,
                    Address = new Address
                    {
                        Street = paymentCheckout.PaymentClient.Address.AddressLines,
                        Number = paymentCheckout.PaymentClient.Address.Number,
                        Complement = paymentCheckout.PaymentClient.Address.Complement,
                        ZipCode = paymentCheckout.PaymentClient.Address.ZipCode,
                        City = paymentCheckout.PaymentClient.Address.City,
                        State = paymentCheckout.PaymentClient.Address.State.Code,
                        Country = "BRA",
                        District = paymentCheckout.PaymentClient.Address.District
                    }
                },
                Payment = new PaymentBankSlip
                {
                    Amount = (int)(paymentCheckout.Value * 100),
                    Address = paymentCheckout.PaymentClient.Address.ToString(),
                    BoletoNumber = paymentCheckout.BankSlipInfo.Number,
                    Assignor = paymentCheckout.BankSlipInfo.Assignor,
                    Demonstrative = paymentCheckout.BankSlipInfo.Demonstrative,
                    ExpirationDate = paymentCheckout.BankSlipInfo.ExpirationDate.ToString("yyyy-MM-dd"),
                    Instructions = paymentCheckout.BankSlipInfo.Instructions
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(cieloRequest, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            var response = await SendTransactional<CieloBankSlipResponse>(content);
            return GetReceiptBankSlip(paymentCheckout, response);
        }

        private PaymentReceiptDto GetReceiptCreditCard(PaymentCheckoutDto paymentCheckout, CieloCreditCardResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentCheckout.PaymentId,
                TenantId = paymentCheckout.TenantId,
                UserId = paymentCheckout.UserId,
                OrderId = paymentCheckout.OrderId,
                OriginalValue = paymentCheckout.Value,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.AuthorizationCode ?? string.Empty,
                TransactionId = response?.Payment.Tid ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.ReturnMessage ?? string.Empty,
                ProviderPaymentCode = response?.Payment.ReturnCode ?? string.Empty,
                ProofOfSale = response?.Payment.ProofOfSale ?? string.Empty,
                ProviderKey = paymentCheckout.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.CreditCard,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptDebitCard(PaymentCheckoutDto paymentCheckout, CieloDebitCardResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentCheckout.PaymentId,
                TenantId = paymentCheckout.TenantId,
                UserId = paymentCheckout.UserId,
                OrderId = paymentCheckout.OrderId,
                OriginalValue = paymentCheckout.Value,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.AuthorizationCode ?? string.Empty,
                TransactionId = response?.Payment.Tid ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.ReturnMessage ?? string.Empty,
                ProviderPaymentCode = response?.Payment.ReturnCode ?? string.Empty,
                ProofOfSale = response?.Payment.ProofOfSale ?? string.Empty,
                ProviderKey = paymentCheckout.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.DebitCard,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptBankSlip(PaymentCheckoutDto paymentCheckout, CieloBankSlipResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentCheckout.PaymentId,
                TenantId = paymentCheckout.TenantId,
                UserId = paymentCheckout.UserId,
                OrderId = paymentCheckout.OrderId,
                OriginalValue = paymentCheckout.Value,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                TransactionId = response?.Payment.BoletoNumber ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.Url ?? string.Empty,
                ProviderKey = paymentCheckout.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.BankSlip,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                },
                BankSlipData = new BankSlipDataDto
                {
                    Url = response?.Payment.Url ?? string.Empty,
                    Bank = response?.Payment.Bank ?? 0,
                    BarCodeNumber = response?.Payment.BarCodeNumber ?? string.Empty,
                    DigitableLine = response?.Payment.DigitableLine ?? string.Empty,
                    ExpirationDate = DateTime.Parse(response?.Payment.ExpirationDate ?? DateTime.UtcNow.ToString()),
                    Country = response?.Payment.Country ?? string.Empty,
                    Currency = response?.Payment.Currency ?? string.Empty,
                    ReceivedDate = DateTime.Parse(response?.Payment.ReceivedDate ?? DateTime.UtcNow.ToString())
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptPix(PaymentCheckoutDto paymentCheckout, CieloPixResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentCheckout.PaymentId,
                TenantId = paymentCheckout.TenantId,
                UserId = paymentCheckout.UserId,
                OrderId = paymentCheckout.OrderId,
                OriginalValue = paymentCheckout.Value,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.ProofOfSale ?? string.Empty,
                TransactionId = response?.Payment.ProofOfSale ?? string.Empty,
                ProviderPaymentCode = response?.Payment.ReturnCode ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.ReturnMessage ?? string.Empty,
                ProviderKey = paymentCheckout.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.PIX,
                Status = PaymentStatus.Pending,
                PixData = new PixDataDto
                {
                    AcquirerTransactionId = response?.Payment.AcquirerTransactionId ?? string.Empty,
                    ProofOfSale = response?.Payment.ProofOfSale ?? string.Empty,
                    QrcodeBase64Image = response?.Payment.QrcodeBase64Image ?? string.Empty,
                    QrCodeString = response?.Payment.QrCodeString ?? string.Empty
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptCreditCard(PaymentReceiptDto paymentReceipt, CieloCreditCardResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentReceipt.PaymentId,
                TenantId = paymentReceipt.TenantId,
                UserId = paymentReceipt.UserId,
                OrderId = paymentReceipt.OrderId,
                OriginalValue = paymentReceipt.OriginalValue,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.AuthorizationCode ?? string.Empty,
                TransactionId = response?.Payment.Tid ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.ReturnMessage ?? string.Empty,
                ProviderPaymentCode = response?.Payment.ReturnCode ?? string.Empty,
                ProofOfSale = response?.Payment.ProofOfSale ?? string.Empty,
                ProviderKey = paymentReceipt.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.CreditCard,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptDebitCard(PaymentReceiptDto paymentReceipt, CieloDebitCardResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentReceipt.PaymentId,
                TenantId = paymentReceipt.TenantId,
                UserId = paymentReceipt.UserId,
                OrderId = paymentReceipt.OrderId,
                OriginalValue = paymentReceipt.OriginalValue,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.AuthorizationCode ?? string.Empty,
                TransactionId = response?.Payment.Tid ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.ReturnMessage ?? string.Empty,
                ProviderPaymentCode = response?.Payment.ReturnCode ?? string.Empty,
                ProofOfSale = response?.Payment.ProofOfSale ?? string.Empty,
                ProviderKey = paymentReceipt.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.DebitCard,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptBankSlip(PaymentReceiptDto paymentReceipt, CieloBankSlipResponse? response)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentReceipt.PaymentId,
                TenantId = paymentReceipt.TenantId,
                UserId = paymentReceipt.UserId,
                OrderId = paymentReceipt.OrderId,
                OriginalValue = paymentReceipt.OriginalValue,
                PaymentValue = (response?.Payment.Amount ?? 0 / 100),
                ExternalId = response?.Payment.PaymentId ?? string.Empty,
                PaymentToken = response?.Payment.PaymentId ?? string.Empty,
                AuthorizationCode = response?.Payment.BoletoNumber ?? string.Empty,
                TransactionId = response?.Payment.BoletoNumber ?? string.Empty,
                ProviderPaymentMessage = response?.Payment.Url ?? string.Empty,
                ProviderKey = paymentReceipt.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = PaymentType.BankSlip,
                Status = response?.Payment.Status switch
                {
                    0 => PaymentStatus.Pending,
                    1 => PaymentStatus.Confirmed,
                    2 => PaymentStatus.Confirmed,
                    3 => PaymentStatus.Refused,
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    13 => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Refused
                },
                BankSlipData = new BankSlipDataDto
                {
                    Url = response?.Payment.Url ?? string.Empty,
                    Bank = response?.Payment.Bank ?? 0,
                    BarCodeNumber = response?.Payment.BarCodeNumber ?? string.Empty,
                    DigitableLine = response?.Payment.DigitableLine ?? string.Empty,
                    ExpirationDate = DateTime.Parse(response?.Payment.ExpirationDate ?? DateTime.UtcNow.ToString()),
                    Country = response?.Payment.Country ?? string.Empty,
                    Currency = response?.Payment.Currency ?? string.Empty,
                    ReceivedDate = DateTime.Parse(response?.Payment.ReceivedDate ?? DateTime.UtcNow.ToString())
                }
            };

            switch (result.Status)
            {
                case PaymentStatus.Confirmed:
                    result.ConfirmationDate = DateTime.Now;
                    break;

                case PaymentStatus.Cancelled:
                    result.CancellationDate = DateTime.Now;
                    break;

                case PaymentStatus.Refounded:
                    result.RefoundDate = DateTime.Now;
                    break;

                case PaymentStatus.Refused:
                    result.CancellationDate = DateTime.Now;
                    break;
            }

            return result;
        }

        private PaymentReceiptDto GetReceiptCancel(PaymentReceiptDto paymentReceipt, CancelResponse cancelRespoonse)
        {
            var result = new PaymentReceiptDto
            {
                PaymentId = paymentReceipt.PaymentId,
                TenantId = paymentReceipt.TenantId,
                UserId = paymentReceipt.UserId,
                OrderId = paymentReceipt.OrderId,
                OriginalValue = paymentReceipt.OriginalValue,
                PaymentValue = paymentReceipt.PaymentValue,
                ExternalId = paymentReceipt.ExternalId,
                PaymentToken = paymentReceipt.PaymentToken,
                AuthorizationCode = paymentReceipt.AuthorizationCode,
                TransactionId = paymentReceipt.TransactionId,
                ProviderPaymentMessage = cancelRespoonse.ReturnMessage,
                ProviderPaymentCode = cancelRespoonse.ReturnCode,
                ProofOfSale = paymentReceipt.ProofOfSale,
                ProviderKey = paymentReceipt.ProviderKey,
                CheckoutDate = DateTime.Now,
                Type = paymentReceipt.Type,
                Status = cancelRespoonse.Status switch
                {
                    10 => PaymentStatus.Cancelled,
                    11 => PaymentStatus.Refounded,
                    _ => paymentReceipt.Status
                }
            };

            if (result.Status == PaymentStatus.Cancelled)
            {
                result.CancellationDate = DateTime.Now;
            }
            else if (result.Status == PaymentStatus.Refounded)
            {
                result.RefoundDate = DateTime.Now;
            }

            return result;
        }

        private async Task<T?> SendTransactional<T>(StringContent content)
        {
            httpClient.DefaultRequestHeaders.Add("MerchantId", settings.MerchantId);
            httpClient.DefaultRequestHeaders.Add("MerchantKey", settings.MerchantKey);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(settings.TransactionalUrl, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        private async Task<T?> SendQuery<T>(string paymentId)
        {
            httpClient.DefaultRequestHeaders.Add("MerchantId", settings.MerchantId);
            httpClient.DefaultRequestHeaders.Add("MerchantKey", settings.MerchantKey);
            var response = await httpClient.GetAsync($"{settings.QueryUrl}/{paymentId}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        private async Task<CancelResponse> SendCancel(string paymentId)
        {
            httpClient.DefaultRequestHeaders.Add("MerchantId", settings.MerchantId);
            httpClient.DefaultRequestHeaders.Add("MerchantKey", settings.MerchantKey);
            var response = await httpClient.PutAsync($"{settings.TransactionalUrl}/{paymentId}/void", null);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CancelResponse>(responseContent, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }
    }
}