using EMAS.ViewModel;
using EMAS.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EMAS.View
{
    public static class MenuGenerator
    {
       
        private static List<string> ExtractEquipmentPermissions(List<string> permissionSet)
        {
            permissionSet = (from permissions in permissionSet where permissions.StartsWith("Equipment") select permissions.Remove(0,"Equipment".Length)).ToList();
            return permissionSet;
        }


        public static List<Tool> GenerateToolList(List<string> permissionSet)
        {
            List<Tool> tools = new List<Tool>();

            foreach(string permission in ExtractEquipmentPermissions(permissionSet))
            {
                switch(permission)
                {
                    case "Add":
                        tools.Add(new Tool ("Добавить оборудование", new LaunchEquipmentAdditionWindowCommand()));
                        break;
                    case "Edit":
                        tools.Add(new Tool("Изменить", new LaunchEquipmentEditingWindowCommand()));
                        break;
                }
            }

            return tools;
        }
    }
}
