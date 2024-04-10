using EMAS.Model;
using EMAS.View.AdditionWindow;
using EMAS.ViewModel;
using EMAS.Windows;
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
            vm.PermissionChangeWindowRequested += ShowPermissionWindow;
        }

        private void ShowPermissionWindow(Employee employee)
        {
            PermissionChangerWindow permissionChangerWindow = new();
            PermissionChangerVM permissionChangerVM = new();
            permissionChangerVM.InitValues(employee);
            permissionChangerWindow.DataContext = permissionChangerVM;
            permissionChangerWindow.ShowDialog();
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
