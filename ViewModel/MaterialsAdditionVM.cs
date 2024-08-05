using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Service;
using Service.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public partial class MaterialsAdditionVM : ObservableObject
    {
        public event Action AdditionConfirmed;
        public event Action<string> AdditionFailed;

        private int _currentLocationId;

        [ObservableProperty]
        private MaterialPiece _newMaterialPiece = new();

        [ObservableProperty]
        private RelayCommand _confirmAdditionCommand;

        public static IWindowsDialogueService DialogueService { get; private set; }

        public MaterialsAdditionVM()
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = 0;
        }

        public MaterialsAdditionVM(int locationId)
        {
            ConfirmAdditionCommand = new RelayCommand(ConfirmAddition);
            _currentLocationId = locationId;
        }

        private void ConfirmAddition()
        {
            try 
            {
                DataBaseClient.GetInstance().Add(NewMaterialPiece, _currentLocationId);
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
    }
}
