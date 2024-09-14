using EMAS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using EMAS_Web.Filters;
using Service;
using Service.Connection;
using Service.Connection.DataAccess;

namespace EMAS_Web.Controllers
{
    public class StorableObjectController : Controller
    {


        public IActionResult Material(int locationId = 1)
        {
            List<MaterialPiece> materialList = [];

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is MaterialPiece material)
                {
                    materialList.Add(material);
                }
            }

            ViewBag.LocationId = locationId;
            return View(materialList);
        }
        
        public IActionResult Equipment(int locationId = 1)
        {
            EmployeeDataAccess currentUserInfo = new EmployeeDataAccess();

            List<Equipment> equipmentList = [];
            List<Permission> permissions = (from prm in currentUserInfo.SelectEmployeePermissionsList(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")))
                                           where prm.PermissionType.ToString().StartsWith("Equipment") && prm.LocationId == locationId select prm).ToList();

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }
            List<string> permissionsStrings = [];
            foreach(var permission in permissions)
            {
                switch(permission.PermissionType.ToString())
                {
                    case "EquipmentAdd":
                        permissionsStrings.Add("Добавить");
                        break;
                    case "EquipemntDelete":
                        permissionsStrings.Add("Удалить");
                        break;
                    case "EquipemntEdit":
                        permissionsStrings.Add("Изменить");
                        break;
                }
            }

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.LocationId = locationId;
            ViewBag.PermissionNames = permissionsStrings;
            ViewBag.Permissions = permissions;
            return View(equipmentList);
        }
       

        [AuthorizationFilter]
        public IActionResult AddEquipment()
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));


            return View();
        }

        [HttpPost]
        [AuthorizationFilter]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);
                DataBaseClient.GetInstance().Add(addition);
            }
            catch(Exception excpetion)
            {
                ViewBag.ErrorMessage = excpetion.Message;
                ViewBag.NoError = false;
                return View();
            }
            ViewBag.NoError = true;
            return View();
        }

        public IActionResult Index(int locationId)
        {
            return View();
        }

        public IActionResult History(int storableObjectId)
        {
            return View(DataBaseClient.GetInstance().SelectForStorableObjectId(storableObjectId));
        }

        
    }
}
