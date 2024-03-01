using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    /// <summary>
    /// Stores info about event that happent with equipment.
    /// </summary>
    public abstract class HistoryEntryBase
    {
        /// <summary>
        /// When happend.
        /// </summary>
        private DateTime _date;

        /// <summary>
        /// Who responcible.
        /// </summary>
        private Employee _responcible;

        /// <summary>
        /// Returns date when event happend.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return _date;
            }
            private set
            {
                _date = value;
            }
        }

        /// <summary>
        /// Returns employee that is responcible for event.
        /// </summary>
        public Employee Responcible
        {
            get
            {
                return _responcible;
            }
            set
            {
                _responcible = value;
            }
        }

        /// <summary>
        /// Creates new instane of History Entry.
        /// </summary>
        /// <param name="e">Employee that are responcible for entry.</param>
        public HistoryEntryBase(Employee e)
        {
            this.Responcible = e;
            Date = DateTime.Now;
        }

        /// <summary>
        /// For Testing Purpose Only Delete on Release. <inheritdoc cref="HistoryEntryBase"/>.
        /// </summary>
        /// <param name="e">Employee that are responcible for entry.</param>
        /// <param name="date">Custom date.</param>
        public HistoryEntryBase(Employee e, DateTime date)
        {
            Responcible = e;
            Date = date;
        }

        /// <summary>
        /// Returns string that clealrly defines type of entry.
        /// </summary>
        /// <returns>String that contains info about entry.</returns>
        public abstract override string ToString();
    }
}
