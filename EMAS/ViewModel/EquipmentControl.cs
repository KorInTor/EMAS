using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    internal class EquipmentControl
    {
        EquipmentVM equipmentVM;
        int locationID;

        public EquipmentControl(int Id)
        {
            LocationID = Id;
            equipmentVM = new EquipmentVM();
            equipmentVM.AllowedTools = new System.Collections.ObjectModel.ObservableCollection<Tool>(MenuGenerator.GenerateToolList(DataBaseClient.Permissions[locationID]));
        }

        public EquipmentVM EquipmentViewModel
        {
            get
            {
                return equipmentVM;
            }
            private set
            {

            }
        }

        public int LocationID
        {
            get
            {
                return locationID;
            }
            private set
            {
                locationID = value;
            }
        }
    }
}
