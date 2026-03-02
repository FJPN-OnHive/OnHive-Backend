using EHive.Admin.Models;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Tenants;

namespace EHive.Admin.Services
{
    public interface IAdminService
    {
        event EventHandler<EventArgs> Updated;

        bool IsLoggedIn { get; }

        Settings Settings { get; set; }

        LoggedUserDto? LoggedUser { get; }

        TenantDto? Tenant { get; }

        string TenantSlug { get; set; }

        ITenantsService? TenantsService { get; }

        IRolesService? RolesService { get; }

        IUsersService? UsersService { get; }

        ICoursesService? CoursesService { get; }

        IDisciplinesService? DisciplinesService { get; }

        ILessonsService? LessonsService { get; }

        IExamsService? ExamsService { get; }

        IProductsService? ProductsService { get; }

        IEventsService? EventsService { get; }

        IEventsConfigService? EventsConfigService { get; }

        IAutomationsService? AutomationsService { get; }

        IRedirectsService? RedirectsService { get; }

        IStudentsService StudentsService { get; }

        IStorageService StorageService { get; }

        IVideoService VideoService { get; }

        IPostsService PostsService { get; }

        IDictService DictService { get; }

        IUserGroupsService UserGroupsService { get; }

        IMessagesService MessagesService { get; }

        IMessageChannelsService MessageChannelsService { get; }

        IEmailTemplateService EmailTemplateService { get; }

        ISearchService SearchService { get; }

        IWebhookService WebhookService { get; }

        IUserProfileService UserProfileService { get; }

        ICertificatesService CertificatesService { get; }

        IJobsService JobsService { get; }

        IMauticIntegrationService MauticIntegrationService { get; }

        IStudentReportsService StudentReportsService { get; }

        void notifyUpdate();

        Task<bool> Login(LoginDto login);

        Task Logout(string callback);

        Task ChangePassword(string callback);

        Task VerifyLogin();

        Task SaveSettings();

        Task LoadSettings();
    }
}