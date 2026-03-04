using OnHive.Core.Library.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Core.Library.Entities.Students
{
    public class StudentUser : Student
    {
        public User? User { get; set; }
    }
}