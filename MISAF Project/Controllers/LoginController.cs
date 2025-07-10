using MISAF_Project.Services;
using MISAF_Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IApproverService _approverService;

        public LoginController(IEmployeeService employeeService, IApproverService approverService)
        {
            _employeeService = employeeService;
            _approverService = approverService;
        }

        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(LoginViewModel log)
        {
            if (ModelState.IsValid)
            {
                var employee = _employeeService
                    .QueryEmployee()
                    .AsNoTracking()
                    .FirstOrDefault(e => e.ID_No == log.ID_No && e.Birthdate == log.Birth_Date);

                var endorser = _approverService
                    .QueryApprover()
                    .AsNoTracking()
                    .FirstOrDefault(a => a.ID_No == log.ID_No && a.Endorser_Only == "Y");

                var approver = _approverService
                    .QueryApprover()
                    .AsNoTracking()
                    .FirstOrDefault(a => a.ID_No == log.ID_No && a.Endorser_Only == "N");

                var mis = _approverService
                   .QueryApprover()
                   .AsNoTracking()
                   .FirstOrDefault(a => a.ID_No == log.ID_No && a.MIS == "Y");

                if (employee == null)
                {
                    // Add an error to the ModelState
                    ModelState.AddModelError(string.Empty, "Invalid ID number or birthdate.");
                    return View("Index", log);
                }

                // Store in session (simple authentication for demo purposes)
                Session["EmployeeID"] = log.ID_No;
                Session["EmployeeName"] = employee.Name;
                Session["Birthdate"] = log.Birth_Date;

                if(endorser != null)
                {
                    Session["Endorser"] = endorser.Name;
                }
                if (approver != null)
                {
                    Session["Approver"] = approver.Name;
                }
                if (mis != null)
                {
                    Session["MIS"] = mis.Name;
                }

                if(approver == null && endorser == null && mis == null)
                {
                    Session["Requestor"] = employee.Name;
                }

                return RedirectToAction("Index", "Home");
            }

            return View("Index", log);
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }

    }
}