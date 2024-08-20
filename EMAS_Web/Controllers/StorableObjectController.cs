using Microsoft.AspNetCore.Mvc;
using Model;
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

            return View(equipmentList);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult History(int storableObjectId)
        {
            List<string> events = EventStringBuilder.EventsListToStringList(DataBaseClient.GetInstance().SelectForStorableObjectId(storableObjectId));

            return View(events);
        }
    }
}
