using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    /// <summary>
    /// Stores history entry of equipment reservation.
    /// </summary>
    public class ReservedHistoryEntry : HistoryEntryBase
    {
        public ReservedHistoryEntry(Employee e) : base(e)
        {
        }

        public ReservedHistoryEntry(Employee e, DateTime date) : base(e, date)
        {
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Зарезервирован. Ответственный: {this.Responcible.Fullname}");
        }
    }
}