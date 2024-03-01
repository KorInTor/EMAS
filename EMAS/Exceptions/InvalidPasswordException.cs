using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Exceptions
{
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException() : base ("Пароль неверный")
        {
        }

        public InvalidPasswordException(string? message) : base(message ?? "Пароль неверный")
        {
        }

        public InvalidPasswordException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
