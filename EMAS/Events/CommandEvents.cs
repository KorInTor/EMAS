using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Events
{
    static class CommandEvents
    {
        public delegate void Refresh();
        public static event Refresh? OnRefresh;

        public delegate void LaunchAdminMenu();
        public static event LaunchAdminMenu? OnLaunchAdminMenu;

        public delegate void LaunchEmployeeAdditionMenu();
        public static event LaunchEmployeeAdditionMenu? OnLaunchEmployeeAdditionMenu;

    }
}
