using EMAS.Model;
using EMAS.Service.Command;
using EMAS.Service.Connection;
using EMAS.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EMAS.ViewModel
{
    public class EmployeeAdditionVM
    {
        private string _fullname;
        private string _email;
        private string _username;
        private RelayCommand _confirmAddition;

        public RelayCommand ConfirmAddition
        {
            get
            {
                return _confirmAddition ??= new RelayCommand(param=>AddNewEmployee());
            }
        }

        public void AddNewEmployee()
        {
            if (Fullname == string.Empty || Email == string.Empty || Username == string.Empty)
            {
                MessageBox.Show("Не все поля заполнены.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string password = PasswordGenerator.RandomString(10);
            DataBaseClient.AddNewEmployee(new Employee(Fullname,Email,Username), password);
            Clipboard.SetText(password);
            MessageBox.Show($"Добавление успешно!\r\nПароль сотрудника в вашем буфере обмена.", "Успешно.", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public string Fullname
        {
            get
            {
                return _fullname;
            }
            set
            {
                _fullname = value;
            }
        }

        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }

        public EmployeeAdditionVM() { }
    }
}
