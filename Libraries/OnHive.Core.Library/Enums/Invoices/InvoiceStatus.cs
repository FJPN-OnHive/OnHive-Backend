using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Enums.Invoices
{
    public enum InvoiceStatus
    {
        Pending,
        Requested,
        Authorized,
        Rejected,
        Cancelled,
        PendingCancelation,
        CancelationFailed,
        NotEmmited
    }
}