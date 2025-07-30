using MISAF_Project.Core.Data;
using MISAF_Project.ViewModels;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class BaseController : Controller
    {
        protected string AttachmentsFilePath { get { return Server.MapPath("~/App_Data/Attachments"); } }

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

        protected UserData GetAuthUser()
        {
            return Session["AuthUser"] as UserData;
        }

        protected void SetAuthUser(UserData user)
        {
            Session["AuthUser"] = user;
        }

        protected JsonResult JsonWrap(object data) {
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonSuccess(object data)
        {
            return JsonWrap(new { success = true, data = data });
        }

        protected JsonResult JsonError(object data)
        {
            return JsonWrap(new { success = false, errors = data });
        }
    }
}