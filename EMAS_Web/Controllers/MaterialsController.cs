using Microsoft.AspNetCore.Mvc;

namespace EMAS_Web.Controllers
{
    public class MaterialsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return RedirectToAction("RedirectToAddition", "Materials");
        }

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult RedirectToAddition()
        {
            return View("AddMaterials");
        }
    }
}
