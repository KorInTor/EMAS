using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EMAS_WPF
{
    public interface IWindowsDialogueService
    {
        public void ShowSuccesfullMessage(string message, string header = "Успешно.");
        public void ShowFailMessage(string message, string header = "Ошибка!");
        public void ClipboardSetText(string text);
        public void ShowWindow<T>(object dataContext) where T : Window, new();
        public void ShowWindow<T>() where T : Window, new();
        public void Close();
    }
}
