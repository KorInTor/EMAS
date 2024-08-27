namespace Model.Enum
{
    public enum PermissionType
    {
        DeliveryAccess = 1,
        EquipmentAdd = 2,
        EquipmentEdit = 3,
        MaterialsAdd = 4,
        MaterialsEdit = 5
    }

    public static class PermissionTypeExtensions
    {
        public static string RuLocalString(this PermissionType enumValue)
        {
            switch (enumValue)
            {
                case PermissionType.DeliveryAccess:
                    return "Работа с доставками";
                case PermissionType.EquipmentAdd:
                    return "Добавление оборудования";
                case PermissionType.EquipmentEdit:
                    return "Измененеие оборудования";
                case PermissionType.MaterialsAdd:
                    return "Добавление материалов";
                case PermissionType.MaterialsEdit:
                    return "Изменение материалов";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
