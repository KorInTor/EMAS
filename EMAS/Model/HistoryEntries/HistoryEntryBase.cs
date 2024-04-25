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
        private Employee _responsible;

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

        public string TypeOfAction
        {
            get { return $"Событие"; }
        }

        /// <summary>
        /// Returns employee that is responcible for event.
        /// </summary>
        public Employee Responsible
        {
            get
            {
                return _responsible;
            }
            set
            {
                _responsible = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Employee that are responcible for entry.</param>
        /// <param name="date">Custom date.</param>
        public HistoryEntryBase(Employee e, DateOnly date)
        {
            Responsible = e;
            Date = date;
        }

        /// <summary>
        /// Returns string that clealrly defines type of entry.
        /// </summary>
        /// <returns>String that contains info about entry.</returns>
        public abstract override string ToString();

    }
}
