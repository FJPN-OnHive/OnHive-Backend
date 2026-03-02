using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Enums.Events
{
    public enum AutomationConditionType
    {
        equal,
        notEqual,
        greaterThan,
        greaterThanOrEqual,
        lessThan,
        lessThanOrEqual,
        contains,
        notContains,
        startsWith,
        endsWith,
        notStartsWith,
        notEndsWith,
        isNull,
        isNotNull
    }
}