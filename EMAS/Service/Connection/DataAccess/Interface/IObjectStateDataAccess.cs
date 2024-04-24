using DocumentFormat.OpenXml.Drawing;
using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess.Interface
{
    public interface IObjectStateDataAccess<T> : IDataAccess<T> where T : IObjectState
    {
        void Complete(T objectToComplete);
        void Complete(T[] objectToComplete);
        T SelectById(long Id);
    }
}
