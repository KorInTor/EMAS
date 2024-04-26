using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class DeliveryCreationVM : ObservableObject
    {
        public event Action<Delivery> DeliveryCreated;

        public ObservableCollection<IStorableObject> StorableObjectsInDelivery { get; set; }

        [ObservableProperty]
        private IStorableObject _selectedObject;

        [ObservableProperty]
        private KeyValuePair<int, string> _selectedDestination;

        [ObservableProperty]
        private string _departureComment;

        public Dictionary<int, string> LocationIdDictionary { get; set; }

        private int departureLocationId;

        public IWindowsDialogueService DialogueService { get; set; }

        public DeliveryCreationVM(ObservableCollection<IStorableObject> storableObjectsInDelivery, Dictionary<int, string> locationIdDictionary, int departureLocationId, IWindowsDialogueService windowsDialogueService)
        {
            this.StorableObjectsInDelivery = storableObjectsInDelivery;
            this.LocationIdDictionary = locationIdDictionary;
            this.departureLocationId = departureLocationId;
            this.DialogueService = windowsDialogueService;
            foreach (var location in locationIdDictionary)
            {
                if (location.Key == departureLocationId)
                {
                    DepartureLocationName = location.Value;
                    _ = LocationIdDictionary.Remove(location.Key);
                }

            }
            foreach (var storable in StorableObjectsInDelivery)
            {
                Debug.WriteLine(storable.ShortInfo);
            }
        }

        public DeliveryCreationVM()
        {
        }

        public string DepartureLocationName { get; private set; }

        [RelayCommand]
        private void ConfrimDeliveryCreation()
        {
            DeliveryCreated?.Invoke(new Delivery(0, departureLocationId, SelectedDestination.Key, DepartureComment, DateTime.Now, new(StorableObjectsInDelivery)));
        }

        [RelayCommand]
        private void RemoveSelectedFromDelivery()
        {
            if (StorableObjectsInDelivery.Count == 1)
                DialogueService.Close();
            if (StorableObjectsInDelivery.Last().Id == SelectedObject.Id)
            {
                StorableObjectsInDelivery.RemoveAt(StorableObjectsInDelivery.Count-1);
                SelectedObject = StorableObjectsInDelivery.Last();
            }
            else
            {
                StorableObjectsInDelivery.Remove(SelectedObject);
            }
        }
    }
}
