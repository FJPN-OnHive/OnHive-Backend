using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Students.Domain.Models
{
    public static class EventKeys
    {
        public const string StudentCreated = "StudentCreated";

        public const string StudentUpdated = "StudentUpdated";

        public const string StudentDeleted = "StudentDeleted";

        public const string EnrollmentCreated = "EnrollmentCreated";

        public const string EnrollmentUpdated = "EnrollmentUpdated";

        public const string EnrollmentDeleted = "EnrollmentDeleted";

        public const string AllEnrollmentsDeleted = "AllEnrollmentsDeleted";
    }
}