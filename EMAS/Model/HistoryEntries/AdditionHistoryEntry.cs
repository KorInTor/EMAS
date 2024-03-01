using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    public class AdditionHistoryEntry : HistoryEntryBase
    {
        public AdditionHistoryEntry(Employee e) : base(e)
        {
        }

        public AdditionHistoryEntry(Employee e, DateTime date) : base(e, date)
        {
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Добавил в базу. Ответственный: {this.Responcible.Fullname}");
        }
    }
}
