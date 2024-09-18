using Microsoft.AspNetCore.Mvc;

namespace EMAS_Web.Components
{
    public class TableFilterMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string tableId, string columnIndex, List<string> filterOptions, string columnName)
        {
            ViewBag.TableId = tableId;
            ViewBag.ColumnIndex = columnIndex;
            ViewBag.FilterOptions = filterOptions;
            ViewBag.ColumnName = columnName;

			return View();
        }
    }
}
