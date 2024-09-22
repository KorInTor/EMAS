using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Connection.DataAccess.Interface
{
    public interface IStorableObjectStatusDataAccess<T> where T : IStorableObject
    {
        public void UpdateStatuses(IEnumerable<(int, string)> statuses);

        Dictionary<int, string> SelectStatuses();
    }
}
