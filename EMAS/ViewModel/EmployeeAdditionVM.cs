using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Service;
using Service.Connection;
using Service.Security;
using System.ComponentModel;
using EMAS_WPF;

namespace ViewModel
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
        public static IWindowsDialogueService DialogueService { get; private set; }
        public RelayCommand ConfirmAddition { get; set; }

        public void AddNewEmployee()
        {
            if (NewEmployee.Fullname == string.Empty || NewEmployee.Email == string.Empty || NewEmployee.Username == string.Empty)
            {

                AdditionFailed?.Invoke("Не все поля заполнены.");
                DialogueService.ShowFailMessage("Не все поля заполнены.");
                return;
            }

            string password = PasswordManager.Generate(10);

            try
            {
                NewEmployee.PasswordHash = PasswordManager.Hash(password);
                DataBaseClient.GetInstance().AddSingle(NewEmployee); 
                AdditionSucceeded?.Invoke("Добавление успешно!\r\nПароль сотрудника в вашем буфере обмена.", password);
                DialogueService.ClipboardSetText(password);
                DialogueService.ShowSuccesfullMessage("Добавление успешно!\r\nПароль сотрудника в вашем буфере обмена.");
            }
            catch (Exception ex)
            {
                AdditionFailed?.Invoke(ex.Message);
                DialogueService.ShowFailMessage(ex.Message);
                DialogueService.Close();
            }
        }

        public EmployeeAdditionVM(IWindowsDialogueService dialogueService)
        {
            ConfirmAddition = new RelayCommand(AddNewEmployee, () => CanAddEmployee);
            NewEmployee.PropertyChanged += OnNewEmployeePropertyChanged;
            DialogueService = dialogueService;
        }

        private void OnNewEmployeePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            CanAddEmployee = !(NewEmployee.Fullname == string.Empty || NewEmployee.Username == string.Empty || NewEmployee.Email == string.Empty);
        }
    }
}
