using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using EMAS_Web.Filters;
using Service;
using Service.Connection;
using Service.Connection.DataAccess.Event;
using Microsoft.AspNetCore.Http.Extensions;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class StorableObjectController : Controller
    {
        [HttpGet]
        [LocationFilter]
        public IActionResult AddEquipment(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment");
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);
                DataBaseClient.GetInstance().AddSingle(addition);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleEquipment");
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId});
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult AddMaterial(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial");
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult AddMaterial(MaterialPiece newMaterial, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newMaterial], locationId);
                DataBaseClient.GetInstance().AddSingle(addition);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleMaterial");
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult EditEquipment(int objectId, int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            Equipment? oldEquipment = (Equipment?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([objectId]).FirstOrDefault();
			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
			if (oldEquipment == null)
            {
                TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment", oldEquipment);
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult EditEquipment(Equipment updatedEquipment, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
			Equipment? oldEquipment = (Equipment?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([updatedEquipment.Id]).FirstOrDefault();
			if (oldEquipment == null)
			{
				TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
				return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
			}

			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedEquipment], comment);
            try
            {
                DataBaseClient.GetInstance().AddSingle(dataChangedEvent);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleEquipment");
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult EditMaterial(int objectId,int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав
            MaterialPiece? oldMaterial = (MaterialPiece?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([objectId]).FirstOrDefault();

			if (oldMaterial == null)
			{
				TempData["AlertMessage"] = "Такого материала нет в Базе данных";
				return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
			}

			ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial", oldMaterial);
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult EditMaterial(MaterialPiece updatedMaterial, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            MaterialPiece? oldMaterial = (MaterialPiece?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([updatedMaterial.Id]).FirstOrDefault();

			if (oldMaterial == null)
            {
                TempData["AlertMessage"] = "Такого материала нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedMaterial],comment);

            try
            {
                DataBaseClient.GetInstance().AddSingle(dataChangedEvent);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleMaterial");
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [LocationFilter]
        public IActionResult Index(int locationId)
        {
			ViewBag.PermissionList = (from prm in DataBaseClient.GetInstance().SelectByIds<Employee>([(int)HttpContext.Session.GetInt32("UserId")], nameof(Employee.Id)).First().Permissions
												where prm.LocationId == locationId
												select prm).OrderBy(x => (int)x.PermissionType);

            var storableObjectList = DataBaseClient.GetInstance().SelectStorableObjectOn(locationId);
			ViewBag.LastEvents = DataBaseClient.GetInstance().SelectLastEventsForStorableObjects(storableObjectList);
            ViewBag.LocationId = locationId;
            return View(storableObjectList);
        }

        public IActionResult History(int storableObjectId)
        {
            var events = DataBaseClient.GetInstance().SelectForStorableObjectsIds([storableObjectId])[storableObjectId].OrderByDescending(x => x.DateTime);
			var idEmployeeName = DataBaseClient.GetInstance().SelectByIds<Employee>(events.Select(x => x.EmployeeId).Distinct(), $"{nameof(Employee.Id)}").ToDictionary(x => x.Id, x => x.Fullname);
			ViewBag.EmployeesNames = idEmployeeName;
			ViewBag.DistinctEmployeNames = idEmployeeName.Values.Distinct();
			return View(events);
        }

    }
}