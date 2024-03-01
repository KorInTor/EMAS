using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    /// <summary>
    /// Stores history entry of decommision.
    /// </summary>
    public class DecommissionedHistoryEntry : HistoryEntryBase
    {
        public DecommissionedHistoryEntry(Employee e) : base(e)
        {
        }

        public DecommissionedHistoryEntry(Employee e, DateTime date) : base(e, date)
        {
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Списан. Ответственный: {this.Responcible.Fullname}");
        }
    }
}