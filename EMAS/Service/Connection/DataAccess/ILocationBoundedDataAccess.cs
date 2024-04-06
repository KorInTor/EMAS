using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public interface ILocationBoundedDataAccess<T> : IDataAccess<T> where T: ILocationBounded
    {
        public void AddOnLocation(T item, int locationId);
        public List<T> SelectOnLocation (int locationId);
    }
}
