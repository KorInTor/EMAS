using EMAS.Events;
using EMAS.Service.Events;
using EMAS.View;
using EMAS.View.AdditionWindow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    class WindowManager
    {

        public WindowManager()
        {
            CommandEvents.OnLaunchAdminMenu += LaunchAdminMenuWindow;
            CommandEvents.OnLaunchEmployeeAdditionMenu += LaunchEmployeeAdditionWindow;
            EmployeeRelatedEvents.EmployeeAdditionIsPerformed += LaunchDialogWindow;
        }
        
        public void LaunchAuthorizationWindow()
        {
            
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        public void LaunchAdminMenuWindow()
        {
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
        }

        public void LaunchEmployeeAdditionWindow()
        {
            EmployeeAddition employeeAdditionWindow = new EmployeeAddition();
            employeeAdditionWindow.Show();
        }

        public void LaunchDialogWindow()
        {
            EmployeeAddition dialogue = new();
            dialogue.ShowDialog();
        }
    }
}
