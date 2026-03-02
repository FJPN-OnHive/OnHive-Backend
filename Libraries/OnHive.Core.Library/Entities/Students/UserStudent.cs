using EHive.Core.Library.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Entities.Students
{
    public class UserStudent : User
    {
        public List<Student> Students { get; set; }
    }
}