using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess.Interface
{
    public interface IDataAccess<T> : IDataAccessWithoutAdd<T>
    {
        void Add(T objectToAdd);
        void Add(T[] objectToAdd);
    }
}
