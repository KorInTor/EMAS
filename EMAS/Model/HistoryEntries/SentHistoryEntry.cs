using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.HistoryEntry
{
    /// <summary>
    /// Stores history entry of sending equipment.
    /// </summary>
    public class SentHistoryEntry : HistoryEntryBase
    {
        /// <summary>
        /// <inheritdoc cref="HistoryEntryBase.HistoryEntryBase(Employee)"/> 
        /// </summary>
        /// <param name="e"><inheritdoc cref="HistoryEntryBase.HistoryEntryBase(Employee)"/></param>
        public SentHistoryEntry(Employee e) : base(e)
        {
        }

        public SentHistoryEntry(Employee e, DateTime date) : base(e, date)
        {
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Отправлен. Ответственный: {this.Responcible.Fullname}");
        }
    }
}
