using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Connection;

namespace EMAS_Web.Filters
{
    public class LocationFilter : Attribute, IResourceFilter
    {
        public static readonly int SpecialLocationId = -1;

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var locationIdString = context.HttpContext.Request.Query["locationId"].ToString();
            if (string.IsNullOrEmpty(locationIdString))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }

            if (!Int32.TryParse(locationIdString, out int locationId))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            bool isAllCaseAllowed = actionDescriptor?.MethodInfo.GetCustomAttributes(typeof(AllowSpecialLocationIdAttribute), false).Any() ?? false;

            if (!IsValidLocationId(locationId, isAllCaseAllowed))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }
        }

        private bool IsValidLocationId(int locationId, bool IsAllCaseAllowed)
        {
            if (IsAllCaseAllowed && locationId == SpecialLocationId)
                return true;

            var locations = DataBaseClient.GetInstance().SelectNamedLocations();
            return locations.ContainsKey(locationId);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AllowSpecialLocationIdAttribute : Attribute
    {
    }
}
