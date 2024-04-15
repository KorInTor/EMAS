using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public class ReceivedHistoryEntry : HistoryEntryBase
    {
        public ReceivedHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }

        public string TypeOfAction
        {
            get { return $"Приём"; }
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Принят. Ответственный: {this.Responsible.Fullname}");
        }

    }
}
