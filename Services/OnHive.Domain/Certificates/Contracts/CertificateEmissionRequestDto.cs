using EHive.Core.Library.Contracts.Students;
using EHive.Core.Library.Contracts.Users;

namespace EHive.Core.Library.Contracts.Certificates
{
    public class CertificateEmissionRequestDto
    {
        public string TenantId { get; set; } = string.Empty;

        public string CertificateID { get; set; } = string.Empty;

        public UserDto User { get; set; } = new();

        public StudentDto Student { get; set; } = new();

        public StudentCourseDto Course { get; set; } = new();

        public DateTime? EmissionDate { get; set; }
    }
}