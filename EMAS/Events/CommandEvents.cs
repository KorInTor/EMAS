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

        public delegate void AddEquipmentPiece();
        public static event AddEquipmentPiece? OnAddEquipmentPiece;

        public delegate void DeleteEquipmentPiece();
        public static event DeleteEquipmentPiece? OnDeleteEquipmentPiece;
    }
}
