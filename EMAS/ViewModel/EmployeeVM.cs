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
        }

        private void ChangeEmployeePassword()
        {
            string password = PasswordManager.Generate(10);

            DataBaseClient.UpdateEmployeeData(SelectedEmployee, password);

            PasswordChangedSuccsesfull?.Invoke($"Новый пароль для \"{SelectedEmployee.Fullname}\" находится в вашем буфере обмена.", password);
        }

        private void EditEmployee()
        {
            try
            {
                DataBaseClient.UpdateEmployeeData(SelectedEmployee);
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
            EmployeeList = new ObservableCollection<Employee>(DataBaseClient.GetAllEmployeeData());
        }

        public EmployeeVM()
        {
            EmployeeList = new ObservableCollection<Employee> (DataBaseClient.GetAllEmployeeData());

            AddEmployeeCommand = new RelayCommand(RequestAdditionWindow);
            EditEmployeeCommand = new RelayCommand(EditEmployee,() => IsEmployeeSelected);
            ChangePasswordCommand = new RelayCommand(ChangeEmployeePassword, () => IsEmployeeSelected);
        }
    }
}
