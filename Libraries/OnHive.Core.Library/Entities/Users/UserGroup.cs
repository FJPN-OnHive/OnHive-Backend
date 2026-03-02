using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Entities.Users
{
    public class UserGroup : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> UsersIds { get; set; } = [];
    }
}