using EMAS.Model;
using EMAS.Service.Command;
using EMAS.Service.Connection;
using EMAS.Service.Security;
using EMAS.View.AdditionWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EMAS.ViewModel
{
    public class EmployeeVM : INotifyPropertyChanged
    {
        private ObservableCollection<Employee> _employeeList;
        private Employee _selectedEmployee;
        private RelayCommand _addEmployeeCommand;
        private RelayCommand _editEmployeeCommand;
        private RelayCommand _changePasswordCommand;
        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand AddEmployeeCommand => _addEmployeeCommand ??= new RelayCommand(param => AddNewEmployee());
        public RelayCommand EditEmployeeCommand => _editEmployeeCommand ??= new RelayCommand(param => EditEmployee());
        public RelayCommand ChangePasswordCommand => _changePasswordCommand ??= new RelayCommand(param => ChangeEmployeePassword());

        public ObservableCollection<Employee> EmployeeList
        {
            get 
            { 
                return _employeeList; 
            }
            set 
            { 
                _employeeList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmployeeList)));
            }
        }

        public Employee SelectedEmployee
        {
            get
            {
                return _selectedEmployee;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                _selectedEmployee = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedEmployee)));
            }
        }

        private void ChangeEmployeePassword()
        {
            if (SelectedEmployee == null)
            {
                MessageBox.Show("Не Выбран сотрудник.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string password = PasswordGenerator.RandomString(10);

            DataBaseClient.UpdateEmployeeData(SelectedEmployee, password);
            Clipboard.SetText(password);
            MessageBox.Show($"Новый пароль для \"{SelectedEmployee.Fullname}\" находится в вашем буфере обмена.", "Смена пароля.", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateEmployeeInfo();
        }

        private void EditEmployee()
        {
            if (SelectedEmployee == null)
            {
                MessageBox.Show("Не Выбран сотрудник.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DataBaseClient.UpdateEmployeeData(SelectedEmployee);
            MessageBox.Show("Данные изменены!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateEmployeeInfo();
        }

        private void AddNewEmployee()
        {
            EmployeeAddition dialogue = new();
            dialogue.ShowDialog();
            UpdateEmployeeInfo();
        }

        public EmployeeVM() 
        {
            UpdateEmployeeInfo();
        }

        private void UpdateEmployeeInfo()
        {
            EmployeeList = new(DataBaseClient.GetAllEmployeeData());
        }
    }
}
