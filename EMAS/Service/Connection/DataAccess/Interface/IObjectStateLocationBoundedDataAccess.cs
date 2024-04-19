using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess.Interface
{
    public interface IObjectStateLocationBoundedDataAccess<T> : ILocationBoundedDataAccess<T>, IObjectStateDataAccess<T> where T : IObjectState, ILocationBounded
    {
    }
}
