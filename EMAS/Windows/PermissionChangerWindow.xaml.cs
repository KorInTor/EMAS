using EMAS.View.AdditionWindow;
using EMAS.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EMAS.Windows
{
    /// <summary>
    /// Логика взаимодействия для PermissionChangerWindow.xaml
    /// </summary>
    public partial class PermissionChangerWindow : Window
    {
        public PermissionChangerWindow()
        {
            InitializeComponent();
            PermissionChangerVM.PermissionChangeFailed += ShowFailMessage;
            PermissionChangerVM.PermissionChangeSucessfull += ShowSuccesfullMessage;
        }

        private void ShowSuccesfullMessage(string message)
        {
            MessageBox.Show(message, "Успешно.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowFailMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
