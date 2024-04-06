using DocumentFormat.OpenXml.Drawing;
using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public interface IEquipmentStateDataAccess<T> : IDataAccess<T> where T : IEquipmentState
    {
        void Complete(T objectToComplete);
        void SelectById(long Id);
    }
}
