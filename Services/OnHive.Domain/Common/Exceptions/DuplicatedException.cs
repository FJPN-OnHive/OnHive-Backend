using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Core.Library.Exceptions
{
    public class DuplicatedException : Exception
    {
        public DuplicatedException(string message) : base(message)
        {
        }

        public DuplicatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DuplicatedException()
        {
        }
    }
}