using BlazorMonaco.Editor;
using EHive.Admin.Helpers;
using EHive.Admin.Services;
using EHive.Core.Library.Contracts.Certificates;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EHive.Admin.Pages
{
    public class CertificatesBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        public List<CertificateDto> Certificates { get; set; } = new();

        public CertificateDto? SelectedCertificate { get; set; } = null;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string CertificateSearch { get; set; } = string.Empty;

        public string NewCategory { get; set; } = string.Empty;

        public string NewTag { get; set; } = string.Empty;

        public TimeSpan? PublishTime { get; set; } = DateTime.Now.TimeOfDay;

        public ExportFormats ExportType = ExportFormats.Csv;

        public bool ExportActiveOnly { get; set; } = true;

        public RadzenHtmlEditor Editor { get; set; }

        public List<CertificateParameterDto> Fields { get; } = [
            new CertificateParameterDto
            {
                Value = "Student.Code",
                Key = "{Student.Code}",
                Description = "Código do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.Name",
                Key = "{Student.Name}",
                Description = "Nome do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.Surname",
                Key = "{Student.Surname}",
                Description = "Sobrenome do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.SocialName",
                Key = "{Student.SocialName}",
                Description = "Nome Social do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.Email",
                Key = "{Student.Email}",
                Description = "E-mail do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.Phone",
                Key = "{Student.Phone}",
                Description = "Telefone do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.DocumentType",
                Key = "{Student.DocumentType}",
                Description = "Tipo do documento do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Student.Document",
                Key = "{Student.Document}",
                Description = "Número do documento do Aluno"
            },
            new CertificateParameterDto
            {
                Value = "Course.Id",
                Key = "{Course.Id}",
                Description = "Id do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.Name",
                Key = "{Course.Name}",
                Description = "Nome do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.Description",
                Key = "{Course.Description}",
                Description = "Descrição do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.EnrollmentCode",
                Key = "{Course.EnrollmentCode}",
                Description = "Código de matrícula do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.Progress",
                Key = "{Course.Progress}",
                Description = "Progresso do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.StartDate",
                Key = "{Course.StartDate}",
                Description = "Data de início do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.EndDate",
                Key = "{Course.EndDate}",
                Description = "Data de término do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.StartTime",
                Key = "{Course.StartTime}",
                Description = "Hora de início do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.EndTime",
                Key = "{Course.EndTime}",
                Description = "Hora de término do curso"
            },
            new CertificateParameterDto
            {
                Value = "Course.Thumbnail",
                Key = "{Course.Thumbnail}",
                Description = "Miniatura do curso"
            },

            new CertificateParameterDto
            {
                Value = "Course.Duration",
                Key = "{Course.Duration}",
                Description = "Duração do curso"
            },

            new CertificateParameterDto
            {
                Value = "Tenant.Name",
                Key = "Tenant.Name}",
                Description = "Nome do Tenant (Empresa)"
            },

            new CertificateParameterDto
            {
                Value = "Tenant.Email",
                Key = "{Tenant.Email}",
                Description = "Email do Tenant (Empresa)"
            },

            new CertificateParameterDto
            {
                Value = "Tenant.CNPJ",
                Key = "{Tenant.CNPJ}",
                Description = "CNPJ do Tenant (Empresa)"
            },
            ];

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/certificates");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/certificates");
            }
            await LoadPosts();
            Loading = false;
        }

        public async Task SaveCertificate()
        {
            try
            {
                if (SelectedCertificate != null)
                {
                    SelectedCertificate.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    await AdminService.CertificatesService.Save(SelectedCertificate, string.IsNullOrEmpty(SelectedCertificate.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Certificado salvo com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/certificates");
            }
            catch (DuplicatedException)
            {
                Message = $"O certificado {SelectedCertificate?.Name ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Certificado";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadPosts()
        {
            try
            {
                Certificates = await AdminService.CertificatesService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/certificates");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAsync()
        {
            SelectedCertificate = null;
            StateHasChanged();
        }

        public async Task SelectCertificateAsync(CertificateDto post)
        {
            SelectedCertificate = post;
            StateHasChanged();
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
                        _ => "csv"
                    };
                    var filename = $"posts.{extession}";
                    await JSRuntime.InvokeAsync<object>("downloadFile", filename, bytes);
                }
            }
        }

        public string GetExportLink()
        {
            return AdminService.PostsService.GetExportPostsUrl(ExportType, AdminService?.LoggedUser?.User?.TenantId ?? string.Empty, ExportActiveOnly);
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

        public string RowClassSelectorCertificate(CertificateDto item, int rowIndex)
        {
            return (item.Equals(SelectedCertificate))
                ? "mud-info"
                : "";
        }

        public void CopyToClipboard(string url)
        {
            ClipboardHelper.CopyToClipboard(url);
            Message = "Chave copiada para a área de transferência.";
            ShowMessage = true;
        }
    }
}