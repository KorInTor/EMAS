using Model.Enum;

namespace Model
{
    public class Permission
    {
        public Permission()
        {
        }

        public Permission(int locationId, PermissionType permissionType)
        {
            LocationId = locationId;
            PermissionType = permissionType;
        }

        public int LocationId { get; set; }

        public PermissionType PermissionType { get; set; }
    }
}
