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

namespace EMAS.ViewModel.DeliveryVM
{
    public partial class DeliveryCreationVM : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfrimDeliveryCreationCommand))]
        private bool _canCreate;

        [ObservableProperty]
        private string _departureComment;

        [ObservableProperty]
        private KeyValuePair<int, string> _selectedDestination;

        [ObservableProperty]
        private IStorableObject _selectedObject;

        private int departureLocationId;

        public string DepartureLocationName { get; private set; }

        public IWindowsDialogueService DialogueService { get; set; }

        public Dictionary<int, string> LocationIdDictionary { get; set; }

        public ObservableCollection<IStorableObject> StorableObjectsInDelivery { get; set; }

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
            SelectedDestination = LocationIdDictionary.First();
            CanCreate = false;
        }

        [RelayCommand(CanExecute = nameof(CanCreate))]
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
                StorableObjectsInDelivery.RemoveAt(StorableObjectsInDelivery.Count - 1);
                SelectedObject = StorableObjectsInDelivery.Last();
            }
            else
            {
                StorableObjectsInDelivery.Remove(SelectedObject);
            }
        }

        partial void OnDepartureCommentChanged(string value)
        {
            if (value.Trim(new char[] { ' ', '.', ',', '\r', '\n' }) != string.Empty)
                CanCreate = true;
            else 
                CanCreate = false;
        }

        public event Action<Delivery> DeliveryCreated;
    }
}
