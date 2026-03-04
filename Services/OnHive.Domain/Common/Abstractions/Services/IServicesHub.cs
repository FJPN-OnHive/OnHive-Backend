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

namespace OnHive.Domains.Common.Abstractions.Services
{
    public interface IServicesHub
    {
        ITenantParametersService TenantParametersService { get; }
        ITenantsService TenantsService { get; }
        IUsersService UsersService { get; }
        IUserGroupsService UserGroupsService { get; }
        IStudentsService StudentsService { get; }
        IEmailsService EmailsService { get; }
        IPaymentsService PaymentsService { get; }
        IInvoicesService InvoicesService { get; }
        IProductsService ProductsService { get; }
        ICoursesService CoursesService { get; }
        ICertificatesService CertificatesService { get; }
        IExamsService ExamsService { get; }
        IVideosService VideosService { get; }
        IStorageFilesService StorageFilesService { get; }
        IStorageImagesService StorageImagesService { get; }
        IOrdersService OrdersService { get; }
        IStudentActivitiesService StudentActivitiesService { get; }
    }
}