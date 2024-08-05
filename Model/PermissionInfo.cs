using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Connection
{
    public class PermissionInfo
    {
        public PermissionInfo(bool isCurrentEmployeeAdmin, Dictionary<int, List<string>> permissions)
        {
            IsCurrentEmployeeAdmin = isCurrentEmployeeAdmin;
            Permissions = permissions;
        }
        public PermissionInfo()
        {
            IsCurrentEmployeeAdmin = false;
            Permissions = [];
        }

        public bool IsCurrentEmployeeAdmin { get; private set; }

        public Dictionary<int, List<string>> Permissions { get; private set; } = [];
    }
}
