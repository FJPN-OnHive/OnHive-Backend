using OnHive.Core.Library.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Users.Domain.Models
{
    public class UserWithProfile : User
    {
        public List<UserProfile> Profiles { get; set; } = [];
    }
}