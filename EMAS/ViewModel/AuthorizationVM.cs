using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Exceptions;
using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EMAS.ViewModel
{
    public partial class AuthorizationVM : ObservableObject
    {
        public event Action<string> LoginFailed;
        public event Action LoginSucceeded;

        private RelayCommand _loginCommand;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;
        public static IWindowsDialogueService DialogueService { get; private set; }
        public AuthorizationVM()
        {
            LetMeInCommand = new RelayCommand(FastLogin);
            DialogueService = new WindowsDialogueService();
        }

        public RelayCommand LoginCommand => _loginCommand ??= new RelayCommand(Login);
        public RelayCommand LetMeInCommand { get; }

        private void FastLogin()
        {
            try
            {
                SessionManager.Login("Пряхин", "ps123123");
                LoginSucceeded?.Invoke();
                DataBaseClient.GetInstance().SelectHistoryEntryByEquipmentId(6);
                DialogueService.ShowWindow<MainMenu>();
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
        }

        private void Login()
        {
            try
            {
                SessionManager.Login(Username, Password);
                LoginSucceeded?.Invoke();
                DialogueService.ShowWindow<MainMenu>();
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
        }
    }
}
