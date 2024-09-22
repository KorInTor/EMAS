using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Connection.DataAccess.Interface
{
    public interface IStorableObjectDataAccess<T> where T : IStorableObject
    {
        public IEnumerable<T> SelectByIds(IEnumerable<int> ids);

        public void Add(IEnumerable<T> objectsToAdd);

        public void Delete(IEnumerable<T> objectToDelete);

        public void Update(IEnumerable<T> objectToUpdate);

        public Dictionary<string, List<string>> SelectDistinct(IEnumerable<string>? propertyToSelect = null);
    }
}
