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