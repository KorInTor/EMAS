using EMAS.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Events
{
    public static class MiscellaneousEvents
    {
        public delegate void FillEmployeeInfo(EmployeeListEventArgs e);
        public static event FillEmployeeInfo? EmployeePackageIsReady;

        public delegate void PackEmployeeInfo();
        public static event PackEmployeeInfo? EmployeeInfoIsRequested;

        public delegate void FillLocationInfo(LocationListEventArgs e);
        public static event FillLocationInfo? LocationPackageIsReady;

        public delegate void PackLocationsInfo();
        public static event PackLocationsInfo? LocationPackageIsRequested;

        public static void InvokeEmployeeInfoIsRequested()
        {
            EmployeeInfoIsRequested?.Invoke();
        }

        public static void InvokeEmployeePackageIsReady(EmployeeListEventArgs e)
        {
            EmployeePackageIsReady?.Invoke(e);
        }

        public static void InvokeLocationPackageIsReady(LocationListEventArgs e)
        {
            LocationPackageIsReady?.Invoke(e);
        }

        public static void InvokeLocationPackageIsRequested()
        {
            LocationPackageIsRequested?.Invoke();
        }
    }
}
