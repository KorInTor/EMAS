using EMAS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EMAS.View
{
    internal class MenuAssembler
    {
        public MenuAssembler()
        {
            GeneralEvents.PermissionsOnLocationAreReady += ProcessPermissions;
        }

        public void ProcessPermissions(List<string> permissions)
        {
            List<MenuItem> tools = new List<MenuItem>();
            foreach(string permission in permissions)
            {
                tools.Add(new MenuItem());
                
            }
           
        }
    }
}
