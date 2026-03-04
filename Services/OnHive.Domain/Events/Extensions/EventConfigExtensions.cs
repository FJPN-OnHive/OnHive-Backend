using OnHive.Core.Library.Entities.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Events.Domain.Extensions
{
    public static class EventConfigExtensions
    {
        public static bool Compare(this EventConfig baseEvent, EventConfig otherEvent)
        {
            return baseEvent.Key == otherEvent.Key
                && baseEvent.Origin == otherEvent.Origin
                && baseEvent.Fields.TrueForAll(f => otherEvent.Fields.Exists(of => of.Key == f.Key && of.Description == f.Description));
        }
    }
}