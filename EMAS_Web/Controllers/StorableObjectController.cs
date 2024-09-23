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
    public class StorableObjectController : Controller
    {
        [AuthorizationFilter]
        [LocationFilter]
        public IActionResult AddEquipment(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View();
        }

        [HttpPost]
        [AuthorizationFilter]
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
                ViewBag.ErrorMessage = excpetion.Message;
                ViewBag.NoError = false;
                return View();
            }
            ViewBag.NoError = true;

            return RedirectToActionPermanent("Equipment", "StorableObject", new { locationId = locationId});
        }

        [LocationFilter]
        public IActionResult Index(int locationId)
        {
			ViewBag.PermissionList = (from prm in DataBaseClient.GetInstance().SelectByIds<Employee>([(int)HttpContext.Session.GetInt32("UserId")], nameof(Employee.Id)).First().Permissions
												where prm.LocationId == locationId
												select prm).ToList();

            var storableObjectList = DataBaseClient.GetInstance().SelectStorableObjectOn(locationId);
			ViewBag.LastEvents = DataBaseClient.GetInstance().SelectLastEventsForStorableObjects(storableObjectList);
            ViewBag.LocationId = locationId;
			return View(storableObjectList);
        }

        public IActionResult History(int storableObjectId)
        {
            var events = DataBaseClient.GetInstance().SelectForStorableObjectsIds([storableObjectId])[storableObjectId];
			var idEmployeeName = DataBaseClient.GetInstance().SelectByIds<Employee>(events.Select(x => x.EmployeeId).Distinct(), $"{nameof(Employee.Id)}").ToDictionary(x => x.Id, x => x.Fullname);
			ViewBag.EmployeesNames = idEmployeeName;
			ViewBag.DistinctEmployeNames = idEmployeeName.Values.Distinct();
			return View(events);
        }

    }
}