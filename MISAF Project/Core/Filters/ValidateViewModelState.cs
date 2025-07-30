using System.Linq;
using System.Web.Mvc;

namespace MISAF_Project.Core.Filters
{
    public class ValidateViewModelState : ActionFilterAttribute
    {
        public string ViewName { get; set; } = "Index";
        public string JsonFailMessage { get; set; } = "Invalid data.";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var errors = filterContext.Controller.ViewData.ModelState
                        .Where(ms => ms.Value.Errors.Any())
                        .ToDictionary(
                            ms => ms.Key, // Model property name (e.g., "ID_No")
                            ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToArray() // Array of error messages
                        );

                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = JsonFailMessage, errors },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet, 
                    };
                }
                else 
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = ViewName,
                        ViewData = filterContext.Controller.ViewData,
                        TempData = filterContext.Controller.TempData,
                    };
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}