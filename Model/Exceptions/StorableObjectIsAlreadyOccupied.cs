using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Exceptions
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
