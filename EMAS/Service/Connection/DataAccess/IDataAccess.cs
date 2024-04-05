using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public interface IDataAccess<T>
    {
        void Add(T objectToAdd);
        void Delete(T objectToDelete);
        void Update(T objectToUpdate);
        List<T> Select();
        T SelectById(int id);
    }
}
