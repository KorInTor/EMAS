using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Service
{
    public class WindowsDialogueService : IWindowsDialogueService
    {
        private Window _window;
        public void ShowSuccesfullMessage(string message, string header = "Успешно.")
        {
            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowFailMessage(string message, string header = "Ошибка!")
        {
            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ClipboardSetText(string text)
        {
            Clipboard.SetText(text);
        }

        public void ShowWindow<T>(object dataContext) where T : Window, new()
        {
            _window = new T
            {
                DataContext = dataContext
            };
            _window.ShowDialog();
        }

        public void Close()
        {
            _window.Close();
        }

        public void ShowWindow<T>() where T : Window, new()
        {
            _window = new T();
            _window.ShowDialog();
        }
    }
}
