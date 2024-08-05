using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS_WPF;
using EMAS_WPF.Windows;
using Model.Exceptions;
using Service;
using Service.Connection;

namespace ViewModel
{
    public partial class AuthorizationVM : ObservableObject
    {
        public event Action<string> LoginFailed;
        public event Action LoginSucceeded;
        public event Action LoginStarted;

        private RelayCommand _loginCommand;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;
        public static IWindowsDialogueService DialogueService { get; private set; }
        public AuthorizationVM()
        {
            DialogueService = new WindowsDialogueService();
        }

        [RelayCommand]
        private void FastLogin()
        {
            Username = "Пряхин";
            Password = "ps123123";
            Login();
        }

        [RelayCommand]
        private void Login()
        {
            LoginStarted?.Invoke();
            try
            {
                SessionManager.Login(Username, Password);
            }
            catch (ConnectionFailedException)
            {
                LoginFailed?.Invoke("Проблемы с соединением, обратитесь к администратору.");
                DialogueService.ShowFailMessage("Проблемы с соединением, обратитесь к администратору.");
            }
            catch (InvalidUsernameException)
            {
                LoginFailed?.Invoke("Такого пользователя не существует.");
                DialogueService.ShowFailMessage("Такого пользователя не существует.");
            }
            catch (InvalidPasswordException)
            {
                LoginFailed?.Invoke("Неправильный пароль.");
                DialogueService.ShowFailMessage("Неправильный пароль.");
            }
            LoginSucceeded?.Invoke();
            DialogueService.ShowSuccesfullMessage("Вход успешен собираем данные.");
            DialogueService.ShowWindow<MainWindow>();
        }
    }
}
