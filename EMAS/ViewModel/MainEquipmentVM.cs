﻿using CommunityToolkit.Mvvm.ComponentModel;
using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    /// <summary>
    /// Хранит в себе все VM классы для работы с equipment.
    /// </summary>
    public partial class MainEquipmentVM : ObservableObject
    {
        [ObservableProperty]
        private Location _locationInfo = new();
        public List<string> Permissions { get; set; }
        //New VM goes here.
        public EquipmentVM EquipmentVM { get; set; } = new();
        public DeliveryControlVM DeliveryControlVM { get; set;} = new DeliveryControlVM(typeof(Equipment));
        

        partial void OnLocationInfoChanged(Location value)
        {
            //Updating VMs Data here;
            EquipmentVM.EquipmentSourceList = LocationInfo.Equipments;
            EquipmentVM.ChangeCommandAccess(Permissions);
            DeliveryControlVM.ChagneSourceList(LocationInfo.IncomingDeliveries,LocationInfo.OutgoingDeliveries);
        }
    }
}
