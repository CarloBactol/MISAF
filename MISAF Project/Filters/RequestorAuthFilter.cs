using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Web.Routing;

namespace MISAF_Project.Filters
{
    public class RequestorAuthFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var finalApprover = filterContext.HttpContext.Session?["Requestor"]?.ToString();
            if (filterContext.HttpContext.Session == null ||
                 string.IsNullOrEmpty(finalApprover))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }


        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            // If the result is unauthorized
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                // Create UrlHelper instance
                var urlHelper = new UrlHelper(filterContext.RequestContext);

                // Handle AJAX requests
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { error = "Session expired", redirect = urlHelper.Action("Index", "Login") },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.HttpContext.Response.StatusCode = 401;
                }
                else
                {
                    // Handle non-AJAX requests: Redirect to login page
                    var returnUrl = filterContext.HttpContext.Request.RawUrl;
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                        { "controller", "Login" },
                        { "action", "Index" },
                        { "returnUrl", returnUrl } // Pass returnUrl to login page
                        });
                }
            }
        }
    }
}