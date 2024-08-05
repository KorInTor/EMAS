using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Service;
using Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using EMAS_WPF;

namespace ViewModel
{
    public partial class LocationControlVM : ObservableObject
    {
        public event Action<string> AdditionConfirmed;
        public event Action<string> AdditionFailed;

        [ObservableProperty]
        private ObservableCollection<Location> _locations = new(DataBaseClient.GetInstance().SelectLocations());

        [ObservableProperty]
        private string _newLocationName;

        public RelayCommand AddLocationCommand { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLocationCommand))]
        private bool _isNewNameEmpty;

        public LocationControlVM()
        {
            AddLocationCommand = new RelayCommand(AddNewLocation, () => IsNewNameEmpty);
            DialogueService = new WindowsDialogueService();
        }
        public static IWindowsDialogueService DialogueService { get; private set; }
        private void AddNewLocation()
        {
            try 
            {
                DataBaseClient.GetInstance().Add(new Location(0,NewLocationName));
                AdditionConfirmed?.Invoke("Добавление нового объекта успешно");
                DialogueService.ShowSuccesfullMessage("Добавление нового объекта успешно");
                UpdateLocationsList();
            }
            catch (Exception ex)
            {
                AdditionFailed?.Invoke("Имя не заполнено");
                DialogueService.ShowFailMessage("Имя не заполнено");
            }
        }

        private void UpdateLocationsList()
        {
            Locations = new(DataBaseClient.GetInstance().SelectLocations());
        }
    }
}

