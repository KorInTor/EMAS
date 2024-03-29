﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace EMAS.ViewModel
{
    public partial class LocationControlVM : ObservableObject
    {
        public event Action<string> AdditionConfirmed;
        public event Action<string> AdditionFailed;

        [ObservableProperty]
        private ObservableCollection<Location> _locations = new(DataBaseClient.GetLocationData());

        [ObservableProperty]
        private string _newLocationName;

        public RelayCommand AddLocationCommand { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private bool _isNewNameEmpty;

        public LocationControlVM()
        {
            AddLocationCommand = new RelayCommand(AddNewLocation, () => IsNewNameEmpty);
        }

        private void AddNewLocation()
        {
            try 
            {
                DataBaseClient.AddNewLocation(new Location(0, NewLocationName));
            }
            catch (Exception ex)
            {
                AdditionFailed?.Invoke("Имя не заполнено");
            }
            AdditionConfirmed?.Invoke("Добавление нового объекта успешно");
            
        }

        private void UpdateLocationsList()
        {
            Locations = new(DataBaseClient.GetLocationData());
        }
    }
}

