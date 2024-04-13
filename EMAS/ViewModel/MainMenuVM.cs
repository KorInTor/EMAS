using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class MainMenuVM : ObservableObject
    {
        [ObservableProperty]
        private RelayCommand _openEquipmentWindow;
        [ObservableProperty]
        private RelayCommand _openAboutWindow;
        [ObservableProperty]
        private RelayCommand _openAdminWindow;
        [ObservableProperty]
        private bool _isAdministrator;

        public event Action<int> OpenWindow;
        public static IWindowsDialogueService DialogueService { get; private set; }
        public MainMenuVM()
        {
            IsAdministrator = SessionManager.PermissionInfo.IsCurrentEmployeeAdmin;

            OpenEquipmentWindow = new RelayCommand(RequestEquipmentWindow);
            OpenAboutWindow = new RelayCommand(RequestAboutWindow);
            OpenAdminWindow = new RelayCommand(RequestAdminWindow, () => IsAdministrator);

            DialogueService = new WindowsDialogueService();
        }

        private void RequestEquipmentWindow()
        {
            OpenWindow?.Invoke(0);
            DialogueService.ShowWidnow<EquipmentWindow>();
        }

        private void RequestAboutWindow()
        {
            OpenWindow?.Invoke(1);
        }

        private void RequestAdminWindow()
        {
            OpenWindow?.Invoke(2);
            DialogueService.ShowWidnow<AdminWindow>();
        }
    }
}
