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
        public ReservedHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }

        public string TypeOfAction
        {
            get { return $"Резервация"; }
        }
        
        public override string ToString()
        {
            return ($"Дата: {this.Date} Зарезервирован. Ответственный: {this.Responsible.Fullname}");
        }
    }
}