using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Core.Library.Entities.Users
{
    public class UserGroup : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> UsersIds { get; set; } = [];
    }
}