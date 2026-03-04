using BlazorMonaco.Editor;
using OnHive.Admin.Helpers;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Posts;
using OnHive.Core.Library.Enums.Common;
using OnHive.Core.Library.Enums.Posts;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace OnHive.Admin.Pages
{
    public class PostsBase : ComponentBase
    {
        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IClipboardHelper ClipboardHelper { get; set; }

        public List<BlogPostDto> Posts { get; set; } = new();

        public BlogPostDto? SelectedPost { get; set; } = null;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; } = true;

        public string PostSearch { get; set; } = string.Empty;

        public string NewCategory { get; set; } = string.Empty;

        public string NewTag { get; set; } = string.Empty;

        public string NewSlug { get; set; } = string.Empty;

        public string NewMetadata { get; set; } = string.Empty;

        public TimeSpan? PublishTime { get; set; } = DateTime.Now.TimeOfDay;

        public ExportFormats ExportType = ExportFormats.Csv;

        public bool ExportActiveOnly { get; set; } = true;

        public StandaloneCodeEditor Editor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/posts");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/posts");
            }
            await LoadPosts();
            Loading = false;
        }

        public async Task SavePosts()
        {
            try
            {
                if (SelectedPost != null)
                {
                    SelectedPost.TenantId = AdminService?.LoggedUser?.User?.TenantId ?? string.Empty;
                    SelectedPost.PublishDate = new DateTime(SelectedPost.PublishDate.Value.Year, SelectedPost.PublishDate.Value.Month, SelectedPost.PublishDate.Value.Day, PublishTime.Value.Hours, PublishTime.Value.Minutes, PublishTime.Value.Seconds).ToUniversalTime();
                    SelectedPost.Body = await Editor.GetValue();
                    await AdminService.PostsService.Save(SelectedPost, string.IsNullOrEmpty(SelectedPost.Id), AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = "Post salvo com sucesso.";
                    ShowMessage = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/posts");
            }
            catch (DuplicatedException)
            {
                Message = $"O post {SelectedPost?.Slug ?? string.Empty} já existe.";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao salvar Post";
                ShowMessage = true;
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task LoadPosts()
        {
            try
            {
                Posts = await AdminService.PostsService.GetAll(AdminService?.LoggedUser?.Token ?? string.Empty);
                StateHasChanged();
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/posts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAsync()
        {
            SelectedPost = null;
            StateHasChanged();
        }

        public async Task SelectPostAsync(BlogPostDto post)
        {
            SelectedPost = post;
            post.PublishDate = post.PublishDate.Value.ToLocalTime();
            PublishTime = post.PublishDate.Value.TimeOfDay;
            await Editor.SetValue(SelectedPost.Body);
            StateHasChanged();
        }

        public void ChangePostVisibility(PostVisibility visibility)
        {
            if (SelectedPost != null)
            {
                SelectedPost.Visibility = visibility;
            }
        }

        public void ChangePostStatus(PostStatus status)
        {
            if (SelectedPost != null)
            {
                SelectedPost.Status = status;
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

        public string RowClassSelectorPost(BlogPostDto item, int rowIndex)
        {
            return (item.Equals(SelectedPost))
                ? "mud-info"
                : "";
        }

        public StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "html",
                Value = SelectedPost != null ? SelectedPost.Body : string.Empty,
                Minimap = new EditorMinimapOptions
                {
                    Enabled = false
                },
            };
        }
    }
}