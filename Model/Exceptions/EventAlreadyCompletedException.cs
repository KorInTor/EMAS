using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model.Exceptions
{
    public class EventAlreadyCompletedException : Exception
    {
        public EventAlreadyCompletedException()
        {
        }

        public EventAlreadyCompletedException(string? message) : base(message)
        {
        }

        public EventAlreadyCompletedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
