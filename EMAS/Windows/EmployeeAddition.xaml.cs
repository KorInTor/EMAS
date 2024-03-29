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

namespace EMAS.View.AdditionWindow
{
    /// <summary>
    /// Логика взаимодействия для EmployeeAddition.xaml
    /// </summary>
    public partial class EmployeeAddition : Window
    {
        public EmployeeAddition()
        {
            InitializeComponent();
            EmployeeAdditionVM employeeAdditionVM = (EmployeeAdditionVM)DataContext;
            employeeAdditionVM.AdditionFailed += ShowFailMessage;
            employeeAdditionVM.AdditionSucceeded += ShowSuccesfullMessage;
        }

        private void ShowSuccesfullMessage(string message,string password)
        {
            Clipboard.SetText(password);
            MessageBox.Show(message, "Успешно.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowFailMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
