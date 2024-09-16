using Microsoft.AspNetCore.Mvc;
using Model;

namespace EMAS_Web.Controllers
{
    public class EquipmentPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Reserve()
        {

            return View();
        }

        public IActionResult Add()
        {
            return View("~/Views/StorableObject/AddEquipment.cshtml");
        }

        public IActionResult Edit(Equipment equipmentToEdit)
        {
            return View();
        }

    }
}