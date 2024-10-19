using EMAS_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EMAS_Web.Components
{
    public class TableFilterMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TableFilterViewModel filterViewModel)
        {
			return View(filterViewModel);
        }
    }
}
