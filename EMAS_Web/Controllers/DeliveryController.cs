using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;
using System.Diagnostics;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class DeliveryController : Controller
    {
        public IActionResult Index(int locationId = 1)
        {
            return View(DataBaseClient.GetInstance().GetDeliverysOutOf(locationId));
        }

        [HttpGet]
        public IActionResult Create(IEnumerable<string> selectedIds, int departureId)
        {

            if (selectedIds == null || !selectedIds.Any())
            {
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(id => int.Parse(id)).ToList();

            ViewBag.SelectedIds = selectedIdList;
            List<ValueTuple<int, string>> locationsList = [];
            foreach(var location in DataBaseClient.GetInstance().SelectNamedLocations())
            {
                if (location.Key == departureId)
                {
                    ViewBag.DepartureLocation = (location.Key,location.Value);
                    continue;
                }
                locationsList.Add((location.Key, location.Value));
            }

            ViewBag.Locations = locationsList;
            return View();
        }

        [HttpPost]
        public IActionResult Create(IEnumerable<string> selectedIds, DateTime dateTime, string comment, string destinationName, int destinationId, int departureId)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(id => int.Parse(id));

            var storableObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdList);

            var sentEvent = new SentEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Sent,dateTime, storableObjects, comment, departureId, destinationId);

            DataBaseClient.GetInstance().Add(sentEvent);

            return RedirectToActionPermanent("Index", "Delivery");
        }

        [HttpGet]
        public IActionResult Confirm(long sentEventId)
        {
            var sentEventToConfirm = DataBaseClient.GetInstance().SelectEventById(sentEventId, typeof(SentEvent));

            List<string> shortInfos = [];
            foreach(var item in sentEventToConfirm.ObjectsInEvent)
            {
                shortInfos.Add(item.ShortInfo);
            }
            ViewBag.ObjectsInfo = shortInfos;
            ViewBag.SentEventId = sentEventId;
            return View();
        }

        [HttpPost]
        public IActionResult Confirm(long sentEventId, string comment, DateTime dateTime)
        {
            var sentEventToConfirm = DataBaseClient.GetInstance().SelectEventById(sentEventId, typeof(SentEvent));

            var arrivedEvent = new ArrivedEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Arrived,dateTime.ToUniversalTime(), sentEventToConfirm.ObjectsInEvent, comment,sentEventId);

            try
            {
                DataBaseClient.GetInstance().Add(arrivedEvent);
            }
            catch(Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }

            return RedirectToActionPermanent("Index","Delivery");
        }
    }
}
