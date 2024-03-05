using EMAS.Exceptions;
using EMAS.Service.Command;
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
    public class AuthorizationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private RelayCommand _loginCommand;

        private string _username;
        private string _password;

        public RelayCommand LoginCommand => _loginCommand ??= new RelayCommand(param => Login());

        private void Login()
        {
            try
            {
                DataBaseClient.Login();
            }
            catch(ConnectionFailedException)
            {
                MessageBox.Show("Проблемы с соединением, обратитесь к администратору.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidUsernameException)
            {
                MessageBox.Show("Такого пользователя не существует.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidPasswordException)
            {
                MessageBox.Show("Неправильный пароль.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            MessageBox.Show("Вход успешен");//TODO: Удалить после добавления главного окна. Сейчас нужен только для проверки окна авторизации.
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
                DataBaseClient.Username = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Username)));
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                DataBaseClient.Password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        public AuthorizationVM()
        {
        }

    }
}
