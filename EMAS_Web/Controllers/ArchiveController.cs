using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model;
using Service.Connection.DataAccess.Query;
using Model.Event;
using Service.Connection;


namespace EMAS_Web.Controllers
{
	public class ArchiveController : Controller
	{
		[HttpGet]
		[LocationFilter]
		public IActionResult Index(int locationId, DateTime? floorValue = null, DateTime? ceilingValue = null)
		{
			var events = DataBaseClient.GetInstance().SelectLocationBoundedEvents(locationId, floorValue, ceilingValue);
			var idEmployeeName = DataBaseClient.GetInstance().SelectByIds<Employee>(events.Select(x => x.EmployeeId).Distinct(), $"{nameof(Employee.Id)}").ToDictionary(x => x.Id, x => x.Fullname);
			ViewBag.EmployeesNames = idEmployeeName;
			ViewBag.DistinctEmployeNames = idEmployeeName.Values.Distinct();
			return View(events);
		}

		public IActionResult ObjectsInEvent(int eventId)
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", eventId);

            var currentEvent = DataBaseClient.GetInstance().SelectEvent<StorableObjectEvent>(queryBuilder).FirstOrDefault();

			ViewBag.LastEvents = currentEvent.ObjectsInEvent.ToDictionary(x => x.Id, x => currentEvent);

			return View(currentEvent);
		}
	}
}
