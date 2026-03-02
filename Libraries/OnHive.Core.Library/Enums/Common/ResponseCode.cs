using System.ComponentModel;

namespace EHive.Core.Library.Enums.Common
{
    public enum ResponseCode
    {
        [Description("OK")]
        OK = 0,

        [Description("Empty")]
        Empty = 1,

        [Description("Warning")]
        Warning = 2,

        [Description("Error")]
        Error = 3,

        [Description("Duplicated")]
        Duplicated = 4,

        [Description("EmailNotValidated")]
        EmailNotValidated = 5,

        [Description("Invalid")]
        Invalid = 6,

        [Description("Unauthorized")]
        Unauthorized = 7,

        [Description("Redirect")]
        Redirect = 8
    }
}