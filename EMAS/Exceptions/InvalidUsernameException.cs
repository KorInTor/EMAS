using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Exceptions
{
    public class InvalidUsernameException : Exception
    {
        public InvalidUsernameException()
        : base("Такого имени пользователя не существует")
        {
        }

        public InvalidUsernameException(string? message)
            : base(message ?? "Такого имени пользователя не существует")
        {
        }

        public InvalidUsernameException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
