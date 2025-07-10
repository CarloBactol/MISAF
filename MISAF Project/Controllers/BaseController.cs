using MISAF_Project.Services;
using MISAF_Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class BaseController : Controller
    {
  

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var users = new UserRolesViewModel
            {
                IsRequestor = Session["Requestor"]?.ToString() != null,
                IsEndorser = Session["Endorser"]?.ToString() != null,
                IsApprover = Session["Approver"]?.ToString() != null,
                IsMIS = Session["MIS"]?.ToString() != null
            };

            ViewBag.Users = users;
            base.OnActionExecuting(filterContext);
        }
    }
}