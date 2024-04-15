using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    public class AdditionHistoryEntry : HistoryEntryBase
    {
        public AdditionHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }

        public string TypeOfAction
        {
            get { return $"Добавление"; }
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Добавил в базу. Ответственный: {this.Responsible.Fullname}");
        }

    }
}
