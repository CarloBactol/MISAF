using MISAF_Project.Core.Interfaces;
using MISAF_Project.Core.ViewModels;
using MISAF_Project.Core.Filters;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateViewModelState]
        public ActionResult SignIn(LoginViewModel data)
        {
            var user = _authService.Attempt(data.ID_No, data.Birth_Date);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid ID number or birthdate.");
                return View("Index", data);
            }

            SetAuthUser(user);

            // [DECPRICATED]
            // FOR COMPATIBILITY PURPOSES
            Session["EmployeeID"] = user.IdNo;
            Session["EmployeeName"] = user.FullName;
            Session["Birthdate"] = user.Birthdate;
            Session["Endorser"] = user.IsEndorser ? user.FullName : null;
            Session["Approver"] = user.IsApprover ? user.FullName : null;
            Session["MIS"] = user.IsMIS ? user.FullName : null;
            Session["Requestor"] = user.IsRequestor ? user.FullName : null;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}