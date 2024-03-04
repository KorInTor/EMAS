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
        private DateOnly _date;

        /// <summary>
        /// Who responcible.
        /// </summary>
        private Employee _responcible;

        /// <summary>
        /// Returns date when event happend.
        /// </summary>
        public DateOnly Date
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
        /// 
        /// </summary>
        /// <param name="e">Employee that are responcible for entry.</param>
        /// <param name="date">Custom date.</param>
        public HistoryEntryBase(Employee e, DateOnly date)
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
