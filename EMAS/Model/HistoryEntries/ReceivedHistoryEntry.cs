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
    public class DecommissionedHistoryEntrycs : HistoryEntryBase
    {
        public DecommissionedHistoryEntrycs(Employee e) : base(e)
        {
        }

        public DecommissionedHistoryEntrycs(Employee e, DateTime date) : base(e, date)
        {
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Списан. Ответственный: {this.Responcible.Fullname}");
        }
    }
}
