using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Exceptions
{
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException() : base("Невозможно подключиться к базе данных!")
        {
        }

        public ConnectionFailedException(string? message) : base(message ?? "Невозможно подключиться к базе данных!")
        {
        }
    }
}
