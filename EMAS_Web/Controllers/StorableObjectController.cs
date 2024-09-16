using EMAS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using EMAS_Web.Filters;
using Service;
using Service.Connection;
using Service.Connection.DataAccess.Event;

namespace EMAS_Web.Controllers
{
    public class StorableObjectController : Controller
    {


        public IActionResult Material(int locationId = 1)
        {
            List<MaterialPiece> materialList = [];

            List<Permission> permissionsList = (from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                                where prm.PermissionType.ToString().StartsWith("Material") && prm.LocationId == locationId
                                                select prm).ToList();

            permissionsList.Add((from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                 where prm.PermissionType.ToString() == "DeliveryAccess" && prm.LocationId == locationId
                                 select prm).ToList().First());

            List<string> permissionNames = [];

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is MaterialPiece material)
                {
                    materialList.Add(material);
                }
            }

            foreach (Permission prm in permissionsList)
            {
                switch (prm.PermissionType.ToString())
                {
                    case "MaterialAdd":
                        permissionNames.Add("Добавить");
                        break;

                    case "MaterialEdit":
                        permissionNames.Add("Изменить");
                        break;

                    case "MaterialDelete":
                        permissionNames.Add("Удалить");
                        break;

                }
            }

            ViewBag.PermissionsNames = permissionNames;
            ViewBag.LocationId = locationId;
            return View(materialList);
        }

        public IActionResult Equipment(int locationId = 1)
        {
            //int employeeId = 
            List<Equipment> equipmentList = [];

            List<Location> locations = DataBaseClient.GetInstance().SelectLocations();
            ViewBag.Locations = locations;

            List<Permission> permissionsList = (from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                                where prm.PermissionType.ToString().StartsWith("Equipment") && prm.LocationId == locationId
                                                select prm).ToList();

            permissionsList.Add((from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                 where prm.PermissionType.ToString() == "DeliveryAccess" && prm.LocationId == locationId
                                 select prm).ToList().First());

            List<string> permissionNames = [];

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }

            foreach (Permission prm in permissionsList)
            {
                switch (prm.PermissionType.ToString())
                {
                    case "EquipmentAdd":
                        permissionNames.Add("Добавить");
                        break;

                    case "EquipmentEdit":
                        permissionNames.Add("Изменить");
                        break;

                    case "EquipmentDelete":
                        permissionNames.Add("Удалить");
                        break;

                }
            }
            ViewBag.PermissionNames = permissionNames;
            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.LocationId = locationId;
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
            catch (Exception excpetion)
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