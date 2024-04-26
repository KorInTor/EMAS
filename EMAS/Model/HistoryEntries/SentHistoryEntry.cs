using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    public class SentHistoryEntry : HistoryEntryBase
    {
        public SentHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }
        public string TypeOfAction
        {
            get { return $"Отправление"; }
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Отправлен. Ответственный: {this.Responsible.Fullname}");
        }
    }
}
