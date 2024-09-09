using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMAS_Web.Models
{
    public class EquipmentModel
    {
        public List<Model.Equipment> Equipment
        {
            get;
            set;
        }

        public List<Model.Permission> Permissions
        {
            get;
            set;
        }        
    }
}
