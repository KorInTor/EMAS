using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public interface IEquipmentStateLocationBoundedDataAccess<T> : ILocationBoundedDataAccess<T>, IEquipmentStateDataAccess<T> where T : IEquipmentState, ILocationBounded
    {
    }
}
