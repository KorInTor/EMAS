using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    public class DecommissionedHistoryEntry : HistoryEntryBase
    {
        public DecommissionedHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }

        public string TypeOfAction
        {
            get { return $"Списание"; }
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Списан. Ответственный: {this.Responsible.Fullname}");
        }
    }
}