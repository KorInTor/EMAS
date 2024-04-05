﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class LocationPickerVM : ObservableObject
    {
        public event Action<KeyValuePair<int, string>> LocationSelectionConfirmed;
        public event Action<string> ActionSuccessful;
        public event Action<string> ActionFailed;

        [ObservableProperty]
        private Dictionary<int, string> _locationList = [];

        [ObservableProperty]
        private KeyValuePair<int, string> _selectedLocation;

        [ObservableProperty]
        private string _confirmButtonText;

        [ObservableProperty]
        private string _windowHeaderText;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmSelectionCommand))]
        private bool _canConfirm = true;

        public RelayCommand ConfirmSelectionCommand { get; }

        public LocationPickerVM()
        {
            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection, () => CanConfirm);
        }

        private void ConfirmSelection()
        {
            LocationSelectionConfirmed?.Invoke(SelectedLocation);
        }

        public void Initialize(Dictionary<int, string> locationList, string buttonText = "Подтвердить выбор местоположения", string headerText = "Окно выбора локации")
        {
            LocationList = locationList;

            ConfirmButtonText = buttonText;
            WindowHeaderText = headerText;
        }

        public void ActionResult(bool isActionSuccessful, string message)
        {
            if (isActionSuccessful)
            {
                ActionSuccessful?.Invoke(message);
            }
            else
            {
                ActionFailed?.Invoke(message);
            }
        }
    }
}
