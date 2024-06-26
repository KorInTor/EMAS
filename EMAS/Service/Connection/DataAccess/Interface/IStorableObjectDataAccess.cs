using EMAS.Model;

namespace EMAS.Service.Connection.DataAccess.Interface
{
    public interface IStorableObjectDataAccess<T> where T : IStorableObject
    {
        public void Add(IEnumerable<T> objectsToAdd);
        public void Add(T objectToAdd);
        public void Update(IEnumerable<T> objectsToUpdate);
        public void Update(T objectToUpdate);
        public IEnumerable<T> SelectByIds(IEnumerable<int> ids);
        public T? SelectById(int id);
    }
}
