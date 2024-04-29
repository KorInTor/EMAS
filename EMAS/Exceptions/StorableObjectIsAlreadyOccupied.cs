using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Exceptions
{
    public class StorableObjectIsAlreadyOccupied : Exception
    {
        public StorableObjectIsAlreadyOccupied()
        {
        }

        public StorableObjectIsAlreadyOccupied(string? message) : base(message)
        {
        }
    }
}
