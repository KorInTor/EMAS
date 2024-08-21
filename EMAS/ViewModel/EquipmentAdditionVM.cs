using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Service;
using Service.Connection;
using System.Diagnostics;
using EMAS_WPF;
using Model.Event;

namespace ViewModel
{
    public partial class EquipmentAdditionVM : ObservableObject
    {
        public event Action AdditionConfirmed;
        public event Action<string> AdditionFailed;

        private int _currentLocationId;

        [ObservableProperty]
        private Equipment _newEquipment = new();

        [ObservableProperty]
        private RelayCommand _confirmAdditionCommand;

        // Почему тэги отдельным полем идут??
        [ObservableProperty]
        private string _tags;

        public static IWindowsDialogueService DialogueService { get; private set; }
        public EquipmentAdditionVM()
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = 0;
        }
        public EquipmentAdditionVM(int locationId)
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = locationId;
        }

        private void ConfirmAddition()
        {
            try
            {
                NewEquipment.Tags = [.. Tags.Split('\n')];
                var addition = new AdditionEvent(LocalSessionManager.UserId, 0, EventType.Addition, DateTime.Now, [NewEquipment], _currentLocationId);
                DataBaseClient.GetInstance().Add(addition);
                DialogueService.ShowSuccesfullMessage("Добавленно успешно!");
                AdditionConfirmed?.Invoke();
            }
            catch(Exception exception)
            {
                DialogueService.ShowFailMessage(exception.Message);
                AdditionFailed?.Invoke(exception.Message);
                Debug.WriteLine(exception.Message);
            }
        }

        public void ChangeCurrentLocationId(int id)
        {
            _currentLocationId = id;
        }
    }
}
