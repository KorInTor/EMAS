using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using System.Diagnostics;

namespace EMAS.ViewModel
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

        [ObservableProperty]
        private string _tags;

        public EquipmentAdditionVM(int locationId)
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = locationId;
        }

        public EquipmentAdditionVM()
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = 0;
        }

        private void ConfirmAddition()
        {
            try
            {
                NewEquipment.Tags = [.. Tags.Split('\n')];
                DataBaseClient.AddNewEquipment(NewEquipment, _currentLocationId);
                AdditionConfirmed?.Invoke();
            }
            catch(Exception exception)
            {
                AdditionFailed?.Invoke(exception.Message);
                Debug.WriteLine(exception.Message);
            }
        }
    }
}
