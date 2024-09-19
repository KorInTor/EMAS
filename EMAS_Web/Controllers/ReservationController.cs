using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class ReservationController : Controller
    {
        [HttpGet]
        [LocationFilter]
        public IActionResult Index(int locationId)
        {
            return View(DataBaseClient.GetInstance().SelectReservationOn(locationId));
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult Create(IEnumerable<string> selectedIds, int locationId)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(id => int.Parse(id)).ToList();

            ViewBag.SelectedIds = selectedIdList;

            foreach (var location in DataBaseClient.GetInstance().SelectNamedLocations())
            {
                if (location.Key == locationId)
                {
                    ViewBag.CurrentLocation = (location.Key, location.Value);
                    continue;
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(IEnumerable<string> selectedIds, int locationId,DateTime dateTime, string comment)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(id => int.Parse(id));

            var storableObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdList);

            var reservedEvent = new ReservedEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Reserved,dateTime,storableObjects,comment,locationId);

            try
            {
                DataBaseClient.GetInstance().Add(reservedEvent);
            }
            catch(Exception exception)
            {
                ViewBag.Message = exception.Message;
                return View();
            }

            return RedirectToActionPermanent("Index","Reservation");
        }

        [HttpGet]
        public IActionResult Confirm(long reservedEventId)
        {
            var reservedEventToConfirm = DataBaseClient.GetInstance().SelectEventById(reservedEventId, typeof(ReservedEvent));

            return View(reservedEventToConfirm);
        }

        [HttpPost]
        public IActionResult Confirm(long reservedEventId, string comment, DateTime dateTime, bool isDecomissioned)
        {
            var reservedEventToConfirm = (ReservedEvent)DataBaseClient.GetInstance().SelectEventById(reservedEventId, typeof(ReservedEvent));
            StorableObjectEvent endEvent;
            if (isDecomissioned)
            {
                endEvent = new ReserveEndedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.ReserveEnded, dateTime.ToUniversalTime(), reservedEventToConfirm.ObjectsInEvent, comment, reservedEventId);
            }
            else
            {
                endEvent = new DecomissionedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.Decommissioned, dateTime.ToUniversalTime(), reservedEventToConfirm.ObjectsInEvent, comment, EventType.Reserved, reservedEventId);
            }

            try
            {
                
                DataBaseClient.GetInstance().Add(endEvent);
            }
            catch (Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View(reservedEventToConfirm);
            }

            return RedirectToActionPermanent("Index", "Reservation");
        }
    }
}
