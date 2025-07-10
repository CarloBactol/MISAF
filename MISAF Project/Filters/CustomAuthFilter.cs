using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Filters;
using System.Web.Mvc;

namespace MISAF_Project.Filters
{
    public class CustomAuthFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        // Called before the action executes — checks if user is authenticated
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            // Example condition: check session or any logic you define
            var employeeId = filterContext.HttpContext.Session["EmployeeID"];

            if (employeeId == null)
            {
                // Not authenticated
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        // Called if authentication fails
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                // Redirect to login if not authenticated
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                    { "controller", "Login" },
                    { "action", "Index" }
                    });
            }
        }
    }
}