using EHive.Catalog.Domain.Abstractions.Services;
using EHive.Certificates.Domain.Abstractions.Services;
using EHive.Courses.Domain.Abstractions.Services;
using EHive.Emails.Domain.Abstractions.Services;
using EHive.Invoices.Domain.Abstractions.Services;
using EHive.Orders.Domain.Abstractions.Services;
using EHive.Payments.Domain.Abstractions.Services;
using EHive.Storages.Domain.Abstractions.Services;
using EHive.Students.Domain.Abstractions.Services;
using EHive.Tenants.Domain.Abstractions.Services;
using EHive.Users.Domain.Abstractions.Services;
using EHive.Videos.Domain.Abstractions.Services;
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