using OnHive.Catalog.Domain.Abstractions.Services;
using OnHive.Certificates.Domain.Abstractions.Services;
using OnHive.Courses.Domain.Abstractions.Services;
using OnHive.Emails.Domain.Abstractions.Services;
using OnHive.Invoices.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Abstractions.Services;
using OnHive.Payments.Domain.Abstractions.Services;
using OnHive.Storages.Domain.Abstractions.Services;
using OnHive.Students.Domain.Abstractions.Services;
using OnHive.Tenants.Domain.Abstractions.Services;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.Videos.Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using OnHive.Domains.Common.Abstractions.Services;
using OnHive.Domains.Common.Helpers;

namespace OnHive.Domains.Common.Services
{
    public class ServicesHub : IServicesHub
    {
        public ITenantParametersService TenantParametersService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<ITenantParametersService>();

        public ITenantsService TenantsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<ITenantsService>();

        public IUsersService UsersService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IUsersService>();

        public IStudentsService StudentsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IStudentsService>();

        public IEmailsService EmailsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IEmailsService>();

        public IPaymentsService PaymentsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IPaymentsService>();

        public IInvoicesService InvoicesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IInvoicesService>();

        public IProductsService ProductsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IProductsService>();

        public ICoursesService CoursesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<ICoursesService>();

        public ICertificatesService CertificatesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<ICertificatesService>();

        public IExamsService ExamsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IExamsService>();

        public IVideosService VideosService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IVideosService>();

        public IStorageFilesService StorageFilesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IStorageFilesService>();

        public IStorageImagesService StorageImagesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IStorageImagesService>();

        public IOrdersService OrdersService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IOrdersService>();

        public IUserGroupsService UserGroupsService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IUserGroupsService>();

        public IStudentActivitiesService StudentActivitiesService => ServiceProviderFactory.ServiceProvider!.GetRequiredService<IStudentActivitiesService>();
    }
}