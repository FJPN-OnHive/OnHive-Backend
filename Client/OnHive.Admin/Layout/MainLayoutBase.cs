using Blazored.LocalStorage;
using OnHive.Admin.Helpers;
using OnHive.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;

namespace OnHive.Admin.Layout
{
    public class MainLayoutBase : LayoutComponentBase
    {
        private const string DARK_MODE = "dark_mode";

        [Inject]
        public IAdminService AdminService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        public MudThemeProvider mudThemeProvider;

        public MudTheme Theme = new();

        public bool IsDarkMode { get; set; } = false;

        public bool ShowMessage { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.LoadSettings();
            LoadTheme();
            IsDarkMode = AdminService.Settings.DarkMode;
            await base.OnInitializedAsync();
        }

        public async Task SaveDarkMode()
        {
            AdminService.Settings.DarkMode = !IsDarkMode;
            await AdminService.SaveSettings();
        }

        public async Task OpenStudentArea()
        {
            var link = $"{NavigationManager.BaseUri.Substring(0, NavigationManager.BaseUri.Length - 1).Replace("admin.", $"{AdminService.Tenant.Domain}.")}";
            await JsRuntime.InvokeAsync<string>("openNewTab", link);
        }

        private void LoadTheme()
        {
            Theme = new MudTheme()
            {
                PaletteLight = new PaletteLight()
                {
                    Primary = new MudColor("#F2BC17"),
                    Secondary = new MudColor("#CCE4F7"),
                    AppbarBackground = new MudColor("#f3f3f3"),
                    Background = new MudColor("#f3f3f3"),
                    TextPrimary = new MudColor("#1A1A27"),
                },
                PaletteDark = new PaletteDark()
                {
                    Primary = new MudColor("#F2BC17"),
                    Secondary = new MudColor("#CCE4F7"),
                    AppbarBackground = new MudColor("#1A1A27"),
                    Background = new MudColor("#1A1A27"),
                    TextPrimary = new MudColor("#f3f3f3"),
                }
            };
        }
    }
}