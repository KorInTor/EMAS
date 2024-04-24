using EMAS.Model;
using EMAS.Service.Connection.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class MaterialDataAccess : ILocationBoundedDataAccess<Equipment>
    {
        public void AddOnLocation(Equipment item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void AddOnLocation(Equipment[] item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void Delete(Equipment objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Delete(Equipment[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Equipment> Select()
        {
            throw new NotImplementedException();
        }

        public Equipment SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Equipment> SelectOnLocation(int locationId)
        {
            throw new NotImplementedException();
        }

        public void Update(Equipment objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update(Equipment[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
