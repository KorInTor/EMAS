using DocumentFormat.OpenXml.Bibliography;
using EMAS.EventArgs;
using EMAS.Events;
using EMAS.Model;
using EMAS.Service;
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

        private DataChangeChecker _monitor;

        public ContentDispenser()
        { 
            _monitor = new DataChangeChecker(DataBaseClient.GetLocationsId());
            GeneralEvents.PermissionsOnLocationAreRequested += PackPermissionsInfo;
            GeneralEvents.LocationChanged += HandleLocationChanged;
        }

        public void PackEquipmentInfo()
        {
            List<Equipment> equipment = DataBaseClient.GetEquipmentOnLocation(LocationManager.CurrentLocation.Id);

        }

        public void PackEmployeeInfo()
        {
            List <Employee>  employees= DataBaseClient.GetAllEmployeeData();
            EmployeeRelatedEvents.InvokeEmployeeInfoIsReady(employees);
        }

        public void PackPermissionsInfo()
        {
            List<string> permissions = DataBaseClient.GetPermissions()[LocationManager.CurrentLocation.Id]; // <- Will not work due to it's implementation, and yet other method similar to it is private for some reason
            GeneralEvents.InvokePermissionsOnLocationAreReady(permissions);
        }
        
        public void HandleLocationChanged()
        {
            _monitor.InitListener(LocationManager.CurrentLocation.Id);
        }
        
        
    }
}
