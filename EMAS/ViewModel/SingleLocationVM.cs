using CommunityToolkit.Mvvm.ComponentModel;
using EMAS.Model;
using EMAS.ViewModel.DeliveryVM;
using EMAS.ViewModel.ReservationVM;
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
    public partial class SingleLocationVM : ObservableObject
    {
        [ObservableProperty]
        private Location _locationInfo = new();
        public List<string> Permissions { get; set; }
        //New VM goes here.
        public EquipmentManagerVM EquipmentVM { get; set; } = new();
        public DeliveryControlVM DeliveryControlVM { get; set;} = new ();
        public ReservationControlVM ReservationControlVM { get; set; } = new();
        

        partial void OnLocationInfoChanged(Location value)
        {
            //Updating VMs Data here;
            EquipmentVM.CurrentLocationId = LocationInfo.Id;
            EquipmentVM.ChangeSourceList(SplitByTypes(LocationInfo.StorableObjectsList).Item1);

            EquipmentVM.ChangeCommandAccess(Permissions);
            DeliveryControlVM.ChagneSourceList(LocationInfo.IncomingDeliveries,LocationInfo.OutgoingDeliveries,true);
            ReservationControlVM.ChangeSourceList(LocationInfo.Reservations,true);
        }

        public void UpdateLocationData(Location newLocation)
        {
            LocationInfo = new(newLocation);
        }

        private ValueTuple<List<Equipment>, List<MaterialPiece>> SplitByTypes(IEnumerable<IStorableObject> storableObjects)
        {
            List<Equipment> equipmentList = [];
            List<MaterialPiece> materialList = [];

            foreach (var storableObject in storableObjects)
            {
                if (storableObject is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                    continue;
                }
                if (storableObject is MaterialPiece material)
                {
                    materialList.Add(material);
                    continue;
                }
                throw new NotImplementedException("Данный тип не поддерживается");
            }
            return (equipmentList, materialList);
        }
    }
}
