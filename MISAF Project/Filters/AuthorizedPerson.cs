using System.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Web.Routing;


namespace MISAF_Project.Filters
{
    public class AuthorizedPerson : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            // Check if session exists and required session variables are present
            var requestor = filterContext.HttpContext.Session?["Requestor"]?.ToString();
            var endorser = filterContext.HttpContext.Session?["Endorser"]?.ToString();
            var approver = filterContext.HttpContext.Session?["Approver"]?.ToString();
            var mis = filterContext.HttpContext.Session?["MIS"]?.ToString();

            // If session is null (expired) or none of the required session variables are set
            if (filterContext.HttpContext.Session == null ||
                (string.IsNullOrEmpty(endorser) && string.IsNullOrEmpty(approver) && string.IsNullOrEmpty(mis) && string.IsNullOrEmpty(requestor)))
            {
                filterContext.Result = new HttpUnauthorizedResult(); // Mark as unauthorized
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