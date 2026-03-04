using Blazored.LocalStorage;
using OnHive.Admin.Models;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Tenants;
using Microsoft.AspNetCore.Components;
using System.Dynamic;

namespace OnHive.Admin.Services
{
    public class AdminService : IAdminService
    {
        private const string LOGIN_KEY = "login";
        private const string SETTINGS_KEY = "settings";

        public static AppState AppState { get; private set; } = new AppState();

        private readonly ILoginService loginService;
        private readonly ILocalStorageService localStorageService;
        private readonly NavigationManager navigationManager;

        public ITenantsService? TenantsService { get; private set; }

        public IRolesService? RolesService { get; private set; }

        public IUsersService? UsersService { get; private set; }

        public ICoursesService? CoursesService { get; private set; }

        public IDisciplinesService? DisciplinesService { get; private set; }

        public ILessonsService? LessonsService { get; private set; }

        public IExamsService? ExamsService { get; private set; }

        public IProductsService? ProductsService { get; private set; }

        public IEventsService? EventsService { get; private set; }

        public IAutomationsService? AutomationsService { get; private set; }

        public IEventsConfigService? EventsConfigService { get; private set; }

        public IRedirectsService? RedirectsService { get; private set; }

        public IStudentsService StudentsService { get; private set; }

        public IStorageService StorageService { get; private set; }

        public IVideoService VideoService { get; private set; }

        public IPostsService PostsService { get; private set; }

        public IDictService DictService { get; private set; }

        public IUserGroupsService UserGroupsService { get; private set; }

        public IMessagesService MessagesService { get; private set; }

        public IMessageChannelsService MessageChannelsService { get; private set; }

        public IEmailTemplateService EmailTemplateService { get; private set; }

        public ISearchService SearchService { get; private set; }

        public IWebhookService WebhookService { get; private set; }

        public IUserProfileService UserProfileService { get; private set; }

        public ICertificatesService CertificatesService { get; private set; }

        public IMauticIntegrationService MauticIntegrationService { get; set; }

        public IStudentReportsService StudentReportsService { get; set; }

        public event EventHandler<EventArgs> Updated;

        public AdminService(ILoginService loginService,
                            ILocalStorageService localStorageService,
                            ITenantsService tenantsService,
                            IRolesService rolesService,
                            IUsersService usersService,
                            ICoursesService coursesService,
                            IDisciplinesService disciplinesService,
                            ILessonsService lessonsService,
                            IExamsService examsService,
                            IProductsService productsService,
                            IAutomationsService automationsService,
                            IEventsService eventsService,
                            IEventsConfigService eventsConfigService,
                            IRedirectsService redirectsService,
                            IStudentsService studentsService,
                            IStorageService storageService,
                            IVideoService videoService,
                            IPostsService postsService,
                            IDictService dictService,
                            IUserGroupsService userGroupsService,
                            IMessagesService messagesService,
                            IMessageChannelsService messageChannelsService,
                            IEmailTemplateService emailTemplateService,
                            ISearchService searchService,
                            IWebhookService webhookService,
                            IUserProfileService userProfileService,
                            ICertificatesService certificatesService,
                            IMauticIntegrationService mauticIntegrationService,
                            IStudentReportsService studentReportsService,
                            NavigationManager navigationManager)
        {
            this.loginService = loginService;
            this.localStorageService = localStorageService;
            this.navigationManager = navigationManager;
            UsersService = usersService;
            TenantsService = tenantsService;
            RolesService = rolesService;
            CoursesService = coursesService;
            DisciplinesService = disciplinesService;
            LessonsService = lessonsService;
            ExamsService = examsService;
            ProductsService = productsService;
            AutomationsService = automationsService;
            EventsService = eventsService;
            EventsConfigService = eventsConfigService;
            RedirectsService = redirectsService;
            StudentsService = studentsService;
            StorageService = storageService;
            PostsService = postsService;
            DictService = dictService;
            UserGroupsService = userGroupsService;
            MessagesService = messagesService;
            MessageChannelsService = messageChannelsService;
            EmailTemplateService = emailTemplateService;
            SearchService = searchService;
            WebhookService = webhookService;
            UserProfileService = userProfileService;
            CertificatesService = certificatesService;
            MauticIntegrationService = mauticIntegrationService;
            StudentReportsService = studentReportsService;
        }

        public bool IsLoggedIn => AppState.LoggedUser != null;

        public LoggedUserDto? LoggedUser => AppState.LoggedUser;

        public Settings Settings { get; set; } = new();

        public TenantDto? Tenant => AppState.Tenant;

        public string TenantSlug { get; set; } = string.Empty;

        public async Task VerifyLogin()
        {
            var login = await localStorageService.GetItemAsync<LoggedUserDto>(LOGIN_KEY);
            if (login != null)
            {
                AppState.LoggedUser = login;
                try
                {
                    await LoadTenant();
                }
                catch (UnauthorizedAccessException ex)
                {
                    AppState.LoggedUser = null;
                    await localStorageService.RemoveItemAsync(LOGIN_KEY);
                }
            }
            else
            {
                AppState.LoggedUser = null;
                await localStorageService.RemoveItemAsync(LOGIN_KEY);
            }
        }

        public async Task<bool> Login(LoginDto login)
        {
            try
            {
                var result = await loginService.Login(login);
                var user = await UsersService.GetById(result.User.Id, result.Token);
                AppState.LoggedUser = new LoggedUserDto(user, result.Token);
                await SaveLogin(AppState.LoggedUser);
                await LoadTenant();

                return true;
            }
            catch (Exception ex)
            {
                await Logout(string.Empty);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task LoadTenant()
        {
            if (AppState.LoggedUser != null)
            {
                AppState.Tenant = await TenantsService!.GetTenant(LoggedUser!.User!.TenantId, LoggedUser.Token);
            }
        }

        public async Task Logout(string callback)
        {
            AppState.LoggedUser = null;
            await localStorageService.RemoveItemAsync(LOGIN_KEY);
            if (string.IsNullOrEmpty(callback))
            {
                navigationManager.NavigateTo("/");
            }
            else
            {
                navigationManager.NavigateTo($"/?callback={callback}");
            }
        }

        public async Task ChangePassword(string callback)
        {
            if (string.IsNullOrEmpty(callback))
            {
                navigationManager.NavigateTo("/changepassword");
            }
            else
            {
                navigationManager.NavigateTo($"/changepassword?callback={callback}");
            }
        }

        private async Task SaveLogin(LoggedUserDto login)
        {
            await localStorageService.SetItemAsync(LOGIN_KEY, login);
        }

        public void notifyUpdate()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveSettings()
        {
            await localStorageService.SetItemAsync(SETTINGS_KEY, Settings);
        }

        public async Task LoadSettings()
        {
            if (await localStorageService.ContainKeyAsync(SETTINGS_KEY))
            {
                try
                {
                    Settings = await localStorageService.GetItemAsync<Settings>(SETTINGS_KEY);
                }
                catch (Exception)
                {
                    Settings = new Settings
                    {
                        DarkMode = false,
                        SavedTenant = string.Empty
                    };
                    await SaveSettings();
                }
            }
            else
            {
                Settings = new Settings
                {
                    DarkMode = false,
                    SavedTenant = string.Empty
                };
                await SaveSettings();
            }
        }
    }
}