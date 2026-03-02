using EHive.Core.Library.Enums.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Entities.Events
{
    public class AutomationCondition
    {
        public AutomationConditionType Type { get; set; }

        public string Field { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty;
    }
}