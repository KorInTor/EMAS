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
        }
        
        public void LaunchAuthorizationWindow()
        {
            
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
        }

        public void LaunchAdminMenuWindow()
        {
            AdminWindow adminWindow = new AdminWindow();
        }

        public void LaunchEmployeeAdditionWindow()
        {
            EmployeeAddition employeeAdditionWindow = new EmployeeAddition();
        }
    }
}
