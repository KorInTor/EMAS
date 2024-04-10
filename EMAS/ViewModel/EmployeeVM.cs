using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using EMAS.Service.Security;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace EMAS.ViewModel
{
    public partial class EmployeeVM : ObservableObject
    {
        public event Action AdditionWindowRequested;
        public event Action<string,string> PasswordChangedSuccsesfull;
        public event Action<string> DataChnageSuccesfull;
        public event Action<string> DataChangeFailed;
        public event Action<Employee> PermissionChangeWindowRequested;

        [ObservableProperty]
        private ObservableCollection<Employee> _employeeList;

        [ObservableProperty]
        private Employee _selectedEmployee;

        [ObservableProperty]
        private RelayCommand _changePasswordCommand;

        [ObservableProperty]
        private RelayCommand _addEmployeeCommand;

        [ObservableProperty]
        private RelayCommand _editEmployeeCommand;

        [ObservableProperty]
        private RelayCommand _changePermissionCommand;

        [ObservableProperty]
        private bool _isEmployeeSelected;

        partial void OnSelectedEmployeeChanged(Employee value)
        {
            if (value != null)
            {
                IsEmployeeSelected = true;
            }
            else
            {
                IsEmployeeSelected = false;
            }
        }

        partial void OnIsEmployeeSelectedChanged(bool value)
        {
            ChangePasswordCommand.NotifyCanExecuteChanged();
            AddEmployeeCommand.NotifyCanExecuteChanged();
            EditEmployeeCommand.NotifyCanExecuteChanged();
            ChangePermissionCommand.NotifyCanExecuteChanged();
        }

        private void ChangeEmployeePassword()
        {
            string password = PasswordManager.Generate(10);
            SelectedEmployee.PasswordHash = PasswordManager.Hash(password);
            DataBaseClient.GetInstance().Update(SelectedEmployee);

            PasswordChangedSuccsesfull?.Invoke($"Новый пароль для \"{SelectedEmployee.Fullname}\" находится в вашем буфере обмена.", password);
        }

        private void RequestPermissionsWindow()
        {
            PermissionChangeWindowRequested?.Invoke(SelectedEmployee);
        }

        private void EditEmployee()
        {
            try
            {
                DataBaseClient.GetInstance().Update(SelectedEmployee);
            }
            catch (Exception ex)
            {
                DataChangeFailed?.Invoke($"Ошибка изменения данных: \n\r{ex.Message}");
                return;
            }
            DataChnageSuccesfull?.Invoke("Данные изменены!");
        }

        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke();
            EmployeeList = new ObservableCollection<Employee>(DataBaseClient.GetInstance().SelectEmployee());
        }

        public EmployeeVM()
        {
            EmployeeList = new ObservableCollection<Employee> (DataBaseClient.GetInstance().SelectEmployee());

            AddEmployeeCommand = new RelayCommand(RequestAdditionWindow);
            EditEmployeeCommand = new RelayCommand(EditEmployee,() => IsEmployeeSelected);
            ChangePasswordCommand = new RelayCommand(ChangeEmployeePassword, () => IsEmployeeSelected);
            ChangePermissionCommand = new RelayCommand(RequestPermissionsWindow, () => IsEmployeeSelected);
        }
    }
}
