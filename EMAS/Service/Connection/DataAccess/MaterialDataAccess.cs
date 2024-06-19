using EMAS.Model;
using EMAS.Service.Connection.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class MaterialDataAccess : IStorableObjectDataAccess<MaterialPiece>
    {
        public void Add(IEnumerable<MaterialPiece> objectsToAdd)
        {
            throw new NotImplementedException();
        }

        public void Add(MaterialPiece objectToAdd)
        {
            throw new NotImplementedException();
        }

        public MaterialPiece? SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MaterialPiece> SelectByIds(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MaterialPiece> SelectOnLocation(int locationId)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<MaterialPiece> objectsToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update(MaterialPiece objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocation(MaterialPiece objectToUpdate, int newLocationId)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocation(IEnumerable<MaterialPiece> objectsToUpdate, int newLocationId)
        {
            throw new NotImplementedException();
        }
    }
}
