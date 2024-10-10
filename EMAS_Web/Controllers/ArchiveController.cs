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
        [AllowSpecialLocationId]
        public IActionResult Index(int locationId, DateTime? floorValue = null, DateTime? ceilingValue = null)
		{

			List<StorableObjectEvent> events = []; 
			
            if (locationId == LocationFilter.AllLocationCase)
			{
                foreach (var location in DataBaseClient.GetInstance().SelectNamedLocations())
				{
					events.AddRange(DataBaseClient.GetInstance().SelectLocationBoundedEvents(location.Key, floorValue, ceilingValue));
                }
            }
			else
			{
				events = DataBaseClient.GetInstance().SelectLocationBoundedEvents(locationId, floorValue, ceilingValue);
            }

			var allEmployes = DataBaseClient.GetInstance().Select<Employee>();
			ViewBag.AllEmployeesNames = allEmployes.ToDictionary(x => x.Id, x => $"{x.Fullname}\n{x.Email}");
            var idEmployeeName = allEmployes.Where(x => events.Select(x => x.EmployeeId).Distinct().Contains(x.Id)).ToDictionary(x => x.Id, x => $"{x.Fullname}\n{x.Email}");
			ViewBag.EmployeesNames = idEmployeeName;
			ViewBag.DistinctEmployeNames = idEmployeeName.Values.Distinct();
			return View(events.OrderByDescending(x => x.DateTime));
		}

		public IActionResult EmployeeHistory(int employeeId)
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.LazyInit<StorableObjectEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.EmployeeId)}","=",employeeId);

			var allEmployes = DataBaseClient.GetInstance().Select<Employee>();
			ViewBag.AllEmployeesNames = allEmployes.ToDictionary(x => x.Id, x => $"{x.Fullname}\n{x.Email}");
			ViewBag.SelectedEmloyee = employeeId;
            var idEmployeeName = allEmployes.Where(x => x.Id == employeeId).ToDictionary(x => x.Id, x => $"{x.Fullname}\n{x.Email}");
			ViewBag.EmployeesNames = idEmployeeName;
			ViewBag.DistinctEmployeNames = idEmployeeName.Values.Distinct();

            return View("Index", DataBaseClient.GetInstance().SelectEventsCustom<StorableObjectEvent>(queryBuilder));
		}

		public IActionResult ObjectsInEvent(int eventId)
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", eventId);

            var currentEvent = DataBaseClient.GetInstance().SelectEvent<StorableObjectEvent>(queryBuilder).FirstOrDefault();

			ViewBag.LastEvents = currentEvent.ObjectsInEvent.ToDictionary(x => x.Id, x => currentEvent);

			var responcibleEmployee = DataBaseClient.GetInstance().SelectSingleById<Employee>(currentEvent.EmployeeId, $"{nameof(Employee.Id)}");

            var responicbleInfo = responcibleEmployee.Fullname + "\n" + responcibleEmployee.Email;

			ViewBag.EmployeesNames = new Dictionary<int, string>();
			ViewBag.EmployeesNames.Add(responcibleEmployee.Id, responicbleInfo);

			return View(currentEvent);
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
