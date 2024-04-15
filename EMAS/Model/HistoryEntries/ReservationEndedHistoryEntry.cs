﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMAS.Model.HistoryEntry
{
    public class ReservationEndedHistoryEntry : HistoryEntryBase
    {
        public ReservationEndedHistoryEntry(Employee e, DateOnly date) : base(e, date)
        {
        }

        public string TypeOfAction
        {
            get { return $"Снятие резервации"; }
        }

        public override string ToString()
        {
            return ($"Дата: {this.Date} Закончил резервирование. Ответственный: {this.Responsible.Fullname}");
        }

    }
}