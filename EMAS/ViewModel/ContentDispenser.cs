using EMAS.EventArgs;
using EMAS.Events;
using EMAS.Model;
using EMAS.Service.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    class ContentDispenser
    {

        private DataChangeChecker _checker;
        
        public ContentDispenser()
        {
            //_checker = new DataChangeChecker();
            MiscellaneousEvents.LocationPackageIsRequested += AssembleLocationsInfoPack;
            MiscellaneousEvents.EmployeeInfoIsRequested += AssembleEmployeeInfoPack;
        }

        public void AssembleEmployeeInfoPack()
        {
            List<Employee> employees = DataBaseClient.GetAllEmployeeData();
            MiscellaneousEvents.InvokeEmployeePackageIsReady(new EmployeeListEventArgs(employees));
        }
        
        public void AssembleLocationsInfoPack()
        {
            List<Location> locations = DataBaseClient.GetLocationData();
            MiscellaneousEvents.InvokeLocationPackageIsReady(new LocationListEventArgs(locations));
        }
    }
}
