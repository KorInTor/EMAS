using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Events
{
    public static class GeneralEvents
    {

        public delegate void ReceivePermissionsOnLocation(List<string> permissions);
        public static event ReceivePermissionsOnLocation? PermissionsOnLocationAreReady;

        public delegate void PackPermissionsOnLocation();
        public static event PackPermissionsOnLocation? PermissionsOnLocationAreRequested;

        public delegate void HandleLocationSwitch();
        public static event HandleLocationSwitch? LocationChanged;

        public static void InvokePermissionsOnLocationAreReady(List<string> permissions)
        {
            PermissionsOnLocationAreReady?.Invoke(permissions);
        }

        public static void InvokePermissionsOnLocationAreRequested()
        {
            PermissionsOnLocationAreRequested?.Invoke();
        }

        public static void InvokeLocationChanged()
        {
            LocationChanged?.Invoke();
        }
    }
}
