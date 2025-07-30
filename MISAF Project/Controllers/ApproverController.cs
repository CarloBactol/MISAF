using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Services;
using MISAF_Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace MISAF_Project.Controllers
{
    public class ApproverController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IApproverService _approverService;

        public ApproverController(IUserService userService, IEmployeeService employeeService, IApproverService approverService)
        {
            _userService = userService;
            _employeeService = employeeService;
            _approverService = approverService;
        }


        // GET: Approver
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetLastRecordApprover()
        {
            var approver = _approverService
                .QueryApprover()
                .AsNoTracking()
                .OrderByDescending(a => a.Approver_ID)
                .FirstOrDefault();

            if (approver == null)
            {
                var newApprover = new MAF_Approver
                {
                    Approver_ID = 1
                };
                return Json(new { success = true, data = newApprover }, JsonRequestBehavior.AllowGet);
            }

            // Generate the next ID
            var nextApprover = new MAF_Approver
            {
                Approver_ID = approver.Approver_ID + 1
            };

            return Json(new { success = true, data = nextApprover }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetSavedApprover()
        {
            var approvers = _approverService
                            .QueryApprover()
                            .AsNoTracking()
                            .Where(a => a.Active == "Y")
                            .OrderByDescending(a => a.Approver_ID)
                            .ToList();

            return Json(approvers, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeeById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "ID is required." }, JsonRequestBehavior.AllowGet);
            }

            var employees = _employeeService.GetById(id);
            var users = _userService
                        .QueryUser()
                        .AsNoTracking()
                        .Where(u => u.Active == "Y")
                        .ToList();



            if (employees == null && users == null)
            {
                return Json(new { success = false, message = "Employee not found." }, JsonRequestBehavior.AllowGet);
            }

            var employee = (from emp in employees
                            join user in users on emp.ID_No equals user.ID_No
                            select new
                            {
                                emp.ID_No,
                                emp.Name,
                                Email_CC = user.Email,
                            }).FirstOrDefault();


            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = employee }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetEmployeeByName(string name)
        {
            name = name?.Trim().ToLower();
            if (string.IsNullOrEmpty(name))
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var matched = _employeeService.GetByName(name);

            if (!matched.Any())
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var idNos = matched.Select(e => e.ID_No).ToList();

            var users = _userService
                        .QueryUser()
                        .AsNoTracking()
                        .Where(u => u.Active == "Y")
                        .ToList();

            var result = (from emp in matched
                          join user in users on emp.ID_No equals user.ID_No
                          select new
                          {
                              user.ID_No,
                              user.Email,
                              emp.Name,
                          }).ToList();

            var filterEmail = result.Where(r => !String.IsNullOrEmpty(r.Email)).ToList();

            if (filterEmail.Count == 0)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = filterEmail.Any(),
                data = filterEmail.Distinct().ToList(),
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetEmployeeByFullName(string name)
        {
            name = name?.Trim().ToLower();
            if (string.IsNullOrEmpty(name))
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var matched = _employeeService.GetByName(name);

            if (!matched.Any())
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var idNos = matched.Select(e => e.ID_No).ToList();

            var users = _userService
                        .QueryUser()
                        .AsNoTracking()
                        .Where(u => u.Active == "Y")
                        .ToList();

            var result = (from emp in matched
                          join user in users on emp.ID_No equals user.ID_No
                          select new
                          {
                              user.ID_No,
                              user.Email,
                              emp.Name,
                          }).FirstOrDefault();

            return Json(new
            {
                success = true,
                data = result,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeeByEmail(string email)
        {
            email = email?.Trim().ToLower();
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var relatedUser = _userService
                       .QueryUser()
                       .AsNoTracking()
                       .Where(a => a.Active == "Y" && (a.Email == email || a.Email.Contains(email)))
                       .ToList();

            if (!relatedUser.Any())
                return Json(new { success = false, message = "No user record found." }, JsonRequestBehavior.AllowGet);

            var relatedUserIds = relatedUser.Select(r => r.ID_No).Distinct().ToList();

            var matched = _employeeService
                .QueryEmployee()
                .AsNoTracking()
                .Where(a => relatedUserIds.Contains(a.ID_No))
                .ToList();


            if (!matched.Any())
                return Json(new { success = false, message = "No employee record found." }, JsonRequestBehavior.AllowGet);

            var users = _userService
                        .QueryUser()
                        .AsNoTracking()
                        .Where(u => u.Active == "Y")
                        .ToList();

            var result = (from emp in matched
                          join user in users on emp.ID_No equals user.ID_No
                          select new
                          {
                              user.ID_No,
                              user.Email,
                              emp.Name,
                          }).ToList();

            return Json(new
            {
                success = result.Any(),
                data = result.Distinct().ToList(),
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSelectedEmail(string email)
        {
            email = email?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Invalid email." }, JsonRequestBehavior.AllowGet);
            }

            var user = _userService
                        .QueryUser()
                        .AsNoTracking()
                        .FirstOrDefault(a => a.Active == "Y" && (a.Email == email || a.Email.Contains(email)));

            if (user == null)
            {
                return Json(new { success = false, message = "No user record found." }, JsonRequestBehavior.AllowGet);
            }

            var employees = _employeeService.GetById(user.ID_No);
            if (employees == null || !employees.Any())
            {
                return Json(new { success = false, message = "No employee record found." }, JsonRequestBehavior.AllowGet);
            }

            var relatedUsers = _userService
                                .QueryUser()
                                .AsNoTracking()
                                .Where(a => a.Active == "Y" && (a.Email == user.Email || a.Email.Contains(user.Email)))
                                .ToList();

            var result = (from emp in employees
                          join u in relatedUsers on emp.ID_No equals u.ID_No
                          select new
                          {
                              u.ID_No,
                              u.Email,
                              emp.Name
                          }).FirstOrDefault();

            return Json(new
            {
                success = true,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }



        // ============================ CRUD Management =================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddApprover(ApproverDto approverDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        _approverService.Add(approverDto);

                        var approvers = _approverService.QueryApprover()
                            .Where(a => a.Active == "Y")
                            .OrderBy(a => a.Name)
                            .ToList();

                        scope.Complete();

                        return Json(new { success = true, data = approvers });
                    }
                }

                return Json(new { success = false, message = "Invalid data." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult EditApprover(int id)
        {
            try
            {
                var approver = _approverService.GetApproverById(id);
                return Json(new { success = true, data = approver }, JsonRequestBehavior.AllowGet);
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while searching Approver ID." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApprover(ApproverDto approverDto)
        {
            if (ModelState.IsValid)
            {
                var approver = _approverService.GetApproverById(approverDto.Approver_ID);
                if (approver != null)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                    {
                        _approverService.Update(approverDto);

                        var approvers = _approverService
                               .QueryApprover()
                               .Where(a => a.Active == "Y")
                               .OrderByDescending(a => a.Approver_ID)
                               .ToList();

                        scope.Complete();

                        return Json(new { success = true, data = approvers });
                    }
                }
            }

            return Json(new { success = false, message = "Invalid data." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteApprover(int id)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    _approverService.Delete(id);

                    var approvers = _approverService
                           .QueryApprover()
                           .Where(a => a.Active == "Y")
                           .OrderByDescending(a => a.Approver_ID)
                           .ToList();

                    scope.Complete();

                    return Json(new { success = true, message = "Approver deleted successfully.", data = approvers });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting." });
            }
        }



    }
}