using EMAS.Service.Events;
using System;
using System.Collections.Generic;
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
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
        }

        public void LaunchAdminMenuWindow()
        {
            // Add admin menu window
        }

        public void LaunchEquipmentAdditionWindow()
        {

        }
    }
}
