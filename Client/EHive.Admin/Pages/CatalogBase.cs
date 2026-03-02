using EHive.Admin.Helpers;
using EHive.Admin.Models;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Courses;
using EHive.Core.Library.Contracts.Dict;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Enums.Payments;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EHive.Admin.Pages
{
    public class CatalogBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public List<ProductDto> Products { get; set; } = new();

        public ProductDto? SelectedProduct { get; set; }

        public ProductPriceDto? SelectedPrice { get; set; } = null;

        public List<CourseDto> Courses { get; set; } = [];

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public ProductConversionData? ProductConversionData { get; set; } = null;

        public string ProductSearch { get; set; } = string.Empty;

        public string CourseSearch { get; set; } = string.Empty;

        public string NewCategory { get; set; } = string.Empty;

        public string NewConversionTag { get; set; } = string.Empty;

        public bool ExportActiveOnly { get; set; } = true;

        public DateRange DateRange { get; set; } = new();

        public ExportFormats ExportType = ExportFormats.Csv;

        private InstallmentDto backupInstallment = new();

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/catalog");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/catalog");
            }
            await LoadProducts();
            await LoadCourses();
            Loading = false;
        }

        public async Task SaveProduct()
        {
            try
            {
                if (SelectedProduct == null)
                    return;

                bool existeSkuDuplicado = Products.Any(p =>
                    string.Equals(p.Sku, SelectedProduct.Sku, StringComparison.OrdinalIgnoreCase)
                    && p.Id != SelectedProduct.Id
                );

                if (existeSkuDuplicado)
                {
                    Message = $"O Produto com SKU '{SelectedProduct.Sku}' já existe.";
                    ShowMessage = true;
                    return;
                }

                SelectedProduct.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                await AdminService.ProductsService.Save(
                    SelectedProduct,
                    string.IsNullOrEmpty(SelectedProduct.Id),
                    AdminService?.LoggedUser?.Token ?? string.Empty
                );

                if (ProductConversionData != null)
                {
                    var valueRegistry = await AdminService.DictService
                        .GetCompleteDataAsync(SelectedProduct.TenantId, "Catalog", SelectedProduct.Sku);

                    if (valueRegistry != null)
                    {
                        valueRegistry.Value = JsonSerializer.Serialize(ProductConversionData);
                        await AdminService.DictService
                            .Save(valueRegistry, false, AdminService?.LoggedUser?.Token ?? string.Empty);
                    }
                    else
                    {
                        valueRegistry = new ValueRegistryDto
                        {
                            TenantId = SelectedProduct.TenantId,
                            Group = "Catalog",
                            Key = SelectedProduct.Sku,
                            Value = JsonSerializer.Serialize(ProductConversionData),
                            Category = "ProductConversionData",
                            Name = $"{SelectedProduct.Sku} Conversion Data",
                            Description = $"Conversion Data for {SelectedProduct.Sku}, {SelectedProduct.Name}"
                        };
                        valueRegistry.Value = JsonSerializer.Serialize(ProductConversionData);
                        await AdminService.DictService
                            .Save(valueRegistry, true, AdminService?.LoggedUser?.Token ?? string.Empty);
                    }
                }

                Message = "Produto salvo com sucesso.";
                ShowMessage = true;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/catalog");
            }
            catch (DuplicatedException)
            {
                Message = $"O Produto {SelectedProduct?.Sku ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Produto";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadCourses()
        {
            try
            {
                Courses = await AdminService.CoursesService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/catalog");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadProducts()
        {
            try
            {
                Products = await AdminService.ProductsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/catalog");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task SelectCourse(CourseDto course)
        {
            if (SelectedProduct != null)
            {
                SelectedProduct.Item = course;
                SelectedProduct.ItemId = course.Id;
            }
        }

        public void Clear()
        {
            SelectedProduct = new();
            DateRange = new();
            ProductConversionData = null;
            StateHasChanged();
        }

        public async void SelectProduct(ProductDto product)
        {
            SelectedProduct = product;
            DateRange = new DateRange(SelectedProduct.StartDate, SelectedProduct.EndDate);
            var valueRegistry = await AdminService.DictService.GetCompleteDataAsync(product.TenantId, "Catalog", product.Sku);
            if (valueRegistry != null)
            {
                try
                {
                    ProductConversionData = JsonSerializer.Deserialize<ProductConversionData>(valueRegistry.Value);
                }
                catch (Exception)
                {
                    ProductConversionData = new();
                }
            }
            else
            {
                ProductConversionData = new();
            }
            StateHasChanged();
        }

        public void ChangePaymentType(PaymentType type)
        {
            if (SelectedPrice != null)
            {
                SelectedPrice.PaymentType = type;
            }
        }

        public void BackupInstallment(object installment)
        {
            backupInstallment = new();
            backupInstallment.Installments = ((InstallmentDto)installment).Installments;
            backupInstallment.Value = ((InstallmentDto)installment).Value;
            backupInstallment.Interest = ((InstallmentDto)installment).Interest;
            backupInstallment.Total = ((InstallmentDto)installment).Total;
            StateHasChanged();
        }

        public void ResetInstallment(object installment)
        {
            ((InstallmentDto)installment).Installments = backupInstallment.Installments;
            ((InstallmentDto)installment).Value = backupInstallment.Value;
            ((InstallmentDto)installment).Interest = backupInstallment.Interest;
            ((InstallmentDto)installment).Total = backupInstallment.Total;
            StateHasChanged();
        }

        public async Task RefreshPrices()
        {
            try
            {
                var result = await AdminService.ProductsService.RefreshProductsPricesAsync(AdminService?.LoggedUser?.Token ?? string.Empty);
                Message = $"Preços atualizados";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao atualizar preços";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task ExportAsync()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new("Bearer", AdminService?.LoggedUser?.Token ?? string.Empty);
                var result = await httpClient.GetAsync(exportUrl);
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var bytes = Encoding.UTF8.GetBytes(content);
                    var extession = ExportType switch
                    {
                        ExportFormats.Json => "json",
                        ExportFormats.Xml => "xml",
                        ExportFormats.GoogleXml => "xml",
                        ExportFormats.GoogleTsv => "tsv",
                        _ => "csv"
                    };
                    var filename = $"products.{extession}";
                    await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
                }
            }
        }

        public string GetExportLink()
        {
            return AdminService.ProductsService.GetExportProducUrl(ExportType, AdminService?.LoggedUser?.User?.TenantId ?? string.Empty, ExportActiveOnly);
        }

        public void CopyExportLink()
        {
            var exportUrl = GetExportLink();
            if (!string.IsNullOrEmpty(exportUrl))
            {
                ClipboardHelper.CopyToClipboard(exportUrl);
                Message = $"O Link do arquivo de exportação copiado.";
                ShowMessage = true;
            }
        }

        public void ChangeExportType(ExportFormats format)
        {
            ExportType = format;
        }

        public string RowClassSelector(ProductDto item, int rowIndex)
        {
            return (item.Equals(SelectedProduct))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorCourse(CourseDto item, int rowIndex)
        {
            return (item.Id.Equals(SelectedProduct.ItemId))
                ? "mud-info"
                : "";
        }

        public string RowClassSelectorPrice(ProductPriceDto item, int rowIndex)
        {
            return (item.Equals(SelectedPrice))
                ? "mud-info"
                : "";
        }

        public void GetInscriptionLink(ProductDto product)
        {
            var link = $"{NavigationManager.BaseUri.Substring(0, NavigationManager.BaseUri.Length - 1).Replace("admin.", $"{AdminService.Tenant.Domain}.")}?freeEnrollId={product.Id}";
            ClipboardHelper.CopyToClipboard(link);
            Message = $"O Link de matrícula foi copiado.";
            ShowMessage = true;
        }
    }
}