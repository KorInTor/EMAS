using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using Service;
using Service.Connection;

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
            List<Equipment> equipmentList = [];

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }

            ViewBag.LocationId = locationId;
            return View(equipmentList);
        }

        public IActionResult AddEquipment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
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
