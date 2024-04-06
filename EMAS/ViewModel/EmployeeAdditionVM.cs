using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using EMAS.Service.Security;
using System.ComponentModel;
using System.Windows;

namespace EMAS.ViewModel
{
    public partial class EmployeeAdditionVM : ObservableObject
    {
        public event Action<string> AdditionFailed;
        public event Action<string,string> AdditionSucceeded;

        [ObservableProperty]
        private Employee _newEmployee = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmAddition))]
        private bool _canAddEmployee;

        public RelayCommand ConfirmAddition { get; set; }

        public void AddNewEmployee()
        {
            if (NewEmployee.Fullname == string.Empty || NewEmployee.Email == string.Empty || NewEmployee.Username == string.Empty)
            {

                AdditionFailed?.Invoke("Не все поля заполнены.");
                return;
            }

            string password = PasswordManager.Generate(10);

            try
            {
                NewEmployee.PasswordHash = PasswordManager.Hash(password);
                DataBaseClient.GetInstance().Add(NewEmployee);
            }
            catch (Exception ex)
            {
                AdditionFailed?.Invoke(ex.Message);
            }
            AdditionSucceeded?.Invoke("Добавление успешно!\r\nПароль сотрудника в вашем буфере обмена.",password);
        }

        public EmployeeAdditionVM()
        {
            ConfirmAddition = new RelayCommand(AddNewEmployee, () => CanAddEmployee);
            NewEmployee.PropertyChanged += OnNewEmployeePropertyChanged;
        }

        private void OnNewEmployeePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CanAddEmployee = !(NewEmployee.Fullname == string.Empty || NewEmployee.Username == string.Empty || NewEmployee.Email == string.Empty);
        }
    }
}
