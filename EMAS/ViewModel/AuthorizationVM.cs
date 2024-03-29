using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Exceptions;
using EMAS.Service.Connection;
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
        private string _username = "Пряхин Д.С.";

        [ObservableProperty]
        private string _password = "ps123123";

        public RelayCommand LoginCommand => _loginCommand ??= new RelayCommand(Login);

        private void Login()
        {
            DataBaseClient.Username = Username;
            DataBaseClient.Password = Password;
            try
            {
                DataBaseClient.Login();
                LoginSucceeded?.Invoke();
            }
            catch (ConnectionFailedException)
            {
                LoginFailed?.Invoke("Проблемы с соединением, обратитесь к администратору.");
            }
            catch (InvalidUsernameException)
            {
                LoginFailed?.Invoke("Такого пользователя не существует.");
            }
            catch (InvalidPasswordException)
            {
                LoginFailed?.Invoke("Неправильный пароль.");
            }
        }
    }
}
