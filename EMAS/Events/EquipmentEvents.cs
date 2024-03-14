using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Events
{
    public static class EquipmentEvents
    {
        public delegate void FillEquipmentInfo(List<Equipment> equipment);
        public static event FillEquipmentInfo? EquipmentInfoIsReady;

        public delegate void PackEquipmentInfo();
        public static event PackEquipmentInfo? EquipmentInfoRequested;

        public static void InvokeEquipmentInfoRequested()
        {
            EquipmentInfoRequested?.Invoke();
        }

        public static void InvokeEqiupmentInfoIsReady(List<Equipment> equipment)
        {
            EquipmentInfoIsReady?.Invoke(equipment);
        }
    }
}
