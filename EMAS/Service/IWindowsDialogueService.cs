﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EMAS.Service
{
    public interface IWindowsDialogueService
    {
        public void ShowSuccesfullMessage(string message, string header = "Успешно.");
        public void ShowFailMessage(string message, string header = "Ошибка!");
        public void ClipboardSetText(string text);
        public void ShowWidnow<T>(object dataContext) where T : Window, new();
        public void ShowWidnow<T>() where T : Window, new();
        public void Close();
    }
}
