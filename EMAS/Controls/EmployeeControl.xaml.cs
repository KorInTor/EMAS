using EMAS.View.AdditionWindow;
using EMAS.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace EMAS.View.Control
{
    /// <summary>
    /// Логика взаимодействия для EmployeeControl.xaml
    /// </summary>
    public partial class EmployeeControl : UserControl
    {
        public EmployeeControl()
        {
            InitializeComponent();
            EmployeeVM vm = (EmployeeVM)DataContext;
            vm.AdditionWindowRequested += OpenAdditionWindow;
            vm.PasswordChangedSuccsesfull += ShowPwdChangeSuccesfullMessage;
            vm.DataChnageSuccesfull += ShowSuccesfullMessage;
        }

        private void ShowSuccesfullMessage(string message)
        {
            MessageBox.Show(message, "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowPwdChangeSuccesfullMessage(string message, string password)
        {
            Clipboard.SetText(password);
            MessageBox.Show(message,"Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenAdditionWindow()
        {
            EmployeeAddition employeeAddition = new EmployeeAddition();
            employeeAddition.ShowDialog();
        }
    }
}
