using AutoMapper;
using AutoMapper.Internal;
using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Filters;
using MISAF_Project.Services;
using MISAF_Project.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace MISAF_Project.Controllers
{
    [MISAuthFilter]
    public class AcknowledgeController : BaseController
    {
        private readonly IUserContextService _userContextService;
        private readonly IMainService _mainService;
        private readonly IDetailsService _detailsService;
        private readonly IUserService _userService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IEmployeeService _employeeService;
        private readonly IApproverService _approverService;
        private readonly IAttachmentsService _attachmentsService;

        public AcknowledgeController(IUserContextService userContextService, 
            IMainService mainService,
            IDetailsService detailsService,
            IUserService userService,
            IEmailSenderService emailSenderService,
            IEmployeeService employeeService,
            IApproverService approverService,
            IAttachmentsService attachmentsService)
        {
            _userContextService = userContextService;
            _mainService = mainService;
            _detailsService = detailsService;
            _userService = userService;
            _emailSenderService = emailSenderService;
            _employeeService = employeeService;
            _approverService = approverService;
            _attachmentsService = attachmentsService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetRequestMain()
        {
            try
            {
                var users = new
                {
                    MIS = _userContextService.GetMIS(),
                    UserLogin = _userContextService.GetUserLogin(),
                };

                //var query = _mainService.QueryMain().AsNoTracking();
                //var main = query.OrderBy(m => m.MAF_No).ToList();

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var main = _mainService.QueryMain().AsNoTracking()
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Endorsed_By,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        m.Final_Approver,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();
               

                if (users.MIS != null)
                {
                    main = main.Where(x => (x.Status == "For Acknowledgement MIS" || x.Status == "On Going") && x.Requested_By !=  users.UserLogin).ToList();
                }

                if (users.MIS != null)
                {

                    main = main.Where(x => x.Status == "For Acknowledgement MIS" || x.Status == "On Going").ToList();
                }


                return Json(new { success = true, main, users }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetRequestDetails(string mafNo)
        {  
            try
            {
                if (string.IsNullOrEmpty(mafNo))
                {
                    return Json(new { success = false, message = "Invalid MAF number" }, JsonRequestBehavior.AllowGet);
                }

                var mis = _userContextService.GetMIS();

                var query = _mainService.QueryMain().AsNoTracking();

                if (mis != null)
                {
                    query = query.Where(m => m.MAF_No == mafNo);
                }
                else
                {
                    return Json(new { success = false, message = "No record found" }, JsonRequestBehavior.AllowGet);
                }

                //var main = query.OrderBy(m => m.MAF_No).FirstOrDefault();

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var main = query.ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Endorsed_By,
                        m.DateTime_Endorsed,
                        m.Endorser_Remarks,
                        m.Final_Approver,
                        m.DateTime_Approved,
                        m.Final_Approver_Remarks,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        m.PreApproved,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .FirstOrDefault();




                if (main == null)
                {
                    return Json(new { success = false, message = "No record found" }, JsonRequestBehavior.AllowGet);
                }

                var detail = _detailsService
                    .QueryDetail()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == main.MAF_No)
                    .OrderBy(d => d.MAF_No)
                    .ToList();

                var attachment = _attachmentsService
                    .QueryAttachment()
                    .AsNoTracking()
                    .Where(a => a.MAF_No == main.MAF_No)
                    .OrderBy(d => d.MAF_No)
                    .ToList();

                var users = new
                {
                    MIS = _userContextService.GetMIS(),
                };

                return Json(new { success = true, detail, main, users, attachment }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveAcknowledgeAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Inavalid data." });
                }

                var usersLogin = new
                {
                    userLogin = _userContextService.GetUserLogin(),
                    MIS = _userContextService.GetMIS(),
                };

                var users = _userService
                                  .QueryUser()
                                  .AsNoTracking()
                                  .Where(e => e.Active == "Y")
                                  .ToList();

                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.Record_ID == request.Index && d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                if (existDetails == null && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                var employee = _employeeService
                                             .QueryEmployee()
                                             .AsNoTracking()
                                             .Where(e => e.Name == existMain.Requestor_Name)
                                             .FirstOrDefault();
                List<MAF_Detail> _mapperDetails = null;
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    existDetails.Status = request.Status;
                    existDetails.Status_DateTime = DateTime.Now;
                    existDetails.Status_Remarks = request.Remarks;
                    existDetails.Status_Updated_By = usersLogin.userLogin;
                    await _detailsService.UpdateAsync(existDetails);

                    var details = _detailsService
                                   .QueryDetail()
                                   .AsNoTracking()
                                   .Where(d => d.MAF_No == request.MAF_No)
                                   .ToList();

                    Mapper.CreateMap<MAF_Detail, MAF_Detail>();
                   _mapperDetails =  Mapper.Map<List<MAF_Detail>>(details);

                    var acknowledge = details.Any(d => d.Status == "For Acknowledgement MIS" || d.Status == "On Hold" || d.Status == "On Going");
                    if (!acknowledge)
                    {
                        if (employee != null)
                        {
                            var _remarks = details.Select(s => s.Status_Remarks).ToList();
                            var isApprove = details.Any(s => s.Status == "For Acknowledgement MIS" || s.Status == "On Hold");
                            existMain.Status = isApprove ? "On Going" : "Done";
                            existMain.Status_DateTime = DateTime.Now;
                            existMain.Status_Updated_By = usersLogin.userLogin;
                            existMain.Status_Remarks = string.Join(",", _remarks);
                            existMain.DateTime_Approved = DateTime.Now;
                            existMain.Final_Approver_Remarks = string.Join(",", _remarks);
                            await _mainService.UpdateAsync(existMain);
                        }
                    }
                    scope.Complete();
                }

                var query = _mainService
                             .QueryMain()
                             .Where(d => d.MAF_No == request.MAF_No)
                             .ToList();

                var employees = _employeeService.QueryEmployee().AsNoTracking()
                   .Where(e => !e.Date_Terminated.HasValue)
                   .Select(e => new { e.ID_No, e.Name })
                   .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var _main = query.ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Endorsed_By,
                        m.Endorser_Remarks,
                        m.DateTime_Endorsed,
                        m.DateTime_Requested,
                        m.Final_Approver,
                        m.Final_Approver_Remarks,
                        m.DateTime_Approved,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        m.Requestor_ID_No,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null,
                        Requested_By_ID = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? m.Encoded_By.Split('|')[0].Trim()
                            : null
                    })
                    .FirstOrDefault();

                MAFMainDto main = new MAFMainDto();
                if (_main != null)
                {
                    main.MAF_No = _main.MAF_No;
                    main.Endorsed_By = _main.Endorsed_By;
                    main.DateTime_Endorsed = _main.DateTime_Endorsed;
                    main.DateTime_Requested = _main.DateTime_Requested;
                    main.Endorser_Remarks = _main.Endorser_Remarks;
                    main.Requestor_Name = _main.Requestor_Name;
                    main.Requested_By = _main.Requested_By;
                    main.Status = _main.Status;
                    main.Status_DateTime = _main.Status_DateTime;
                    main.Requested_By_ID = _main.Requested_By_ID;
                    main.Requestor_ID_No = _main.Requestor_ID_No;
                }

                var misUsers = _approverService
                               .QueryApprover()
                               .AsNoTracking()
                               .Where(a => a.MIS == "Y")
                               .Select(a => a.Email_CC)
                               .ToList();


                var requestedFor = users.FirstOrDefault(u => u.ID_No == main.Requestor_ID_No && u.Email != null);
                if (requestedFor != null)
                {
                    misUsers.Add(requestedFor.Email);
                }

                if (!String.IsNullOrWhiteSpace(main.Requested_By) && main.Requested_By != main.Requestor_Name)
                {
                    var requestedBy = users.FirstOrDefault(u => u.ID_No == main.Requested_By_ID && u.Email != null);
                    if (requestedBy != null)
                    {
                        misUsers.Add(requestedBy.Email);
                    }
                }

                var attachments = _attachmentsService
                                    .QueryAttachment()
                                    .AsNoTracking()
                                    .Where(a => a.MAF_No == request.MAF_No)
                                    .ToList();
             
                var mapPath = Server.MapPath("~/App_Data/Attachments");
                if (misUsers.Count > 0)
                {
                    var _detail = _mapperDetails.Where(d => d.Record_ID == request.Index).ToList();
                    if (request.Status == "Done")
                    {
                        main.Status = request.Status;
                        main.Status_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): MIS ACTION FORM IS ALREADY DONE", main, _detail, attachments, mapPath, false, "Acknowledge");
                    }
                    else if (request.Status == "On Hold")
                    {
                        main.Status = request.Status;
                        main.Status_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN PLACED ON HOLD", main, _detail, attachments, mapPath, false, "Acknowledge");
                    }
                    else if (request.Status == "Rejected")
                    {
                        main.Status = request.Status;
                        main.Status_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN REJECTED", main, _detail, attachments, mapPath, false, "Acknowledge");
                    }
                }

                return Json(new { success = true, message = "Request updated sucessfully.", details = _mapperDetails, main, users });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message.ToString() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveAcknowledgeAllAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var userLogin = _userContextService.GetUserLogin();

                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();


                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.MAF_No == request.MAF_No && d.Status == "For Acknowledgement MIS")
                              .ToList();

                if (existDetails.Count == 0 && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    existMain.Status = request.Status;
                    existMain.Status_DateTime = DateTime.Now;
                    existMain.Status_Updated_By = userLogin;
                    await _mainService.UpdateAsync(existMain);

                    foreach (var item in existDetails)
                    {
                        item.Status = request.Status;
                        item.Status_DateTime = DateTime.Now;
                        item.Status_Updated_By = userLogin;
                        await _detailsService.UpdateAsync(item);
                    }
                    scope.Complete();
                }




                var details = _detailsService
                                   .QueryDetail()
                                   .AsNoTracking()
                                   .Where(d => d.MAF_No == request.MAF_No)
                                   .ToList();

                var updateMain = _mainService
                       .QueryMain()
                       .Where(d => d.MAF_No == request.MAF_No)
                       .FirstOrDefault();

                return Json(new { success = true, message = "Request updated sucessfully.", details, main = updateMain });

            }
            catch (Exception ex)
            {
                return Json(new {success = false, message = ex.Message.ToString() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveApproveAllAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var userLogin = _userContextService.GetUserLogin();

                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();


                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.MAF_No == request.MAF_No && (d.Status == "For Acknowledgement MIS" || d.Status == "On Going" || d.Status == "On Hold"))
                              .ToList();

                if (existDetails == null && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                existMain.Status = request.Status;
                existMain.Status_DateTime = DateTime.Now;
                existMain.Status_Updated_By = userLogin;
                await _mainService.UpdateAsync(existMain);

                // Save to History database

                foreach (var item in existDetails)
                {
                    item.Status = request.Status;
                    item.Status_DateTime = DateTime.Now;
                    item.Status_Updated_By = userLogin;
                    await _detailsService.UpdateAsync(item);
                }

                var details = _detailsService
                               .QueryDetail()
                               .AsNoTracking()
                               .Where(d => d.MAF_No == request.MAF_No)
                               .ToList();

                //var updateMain = _mainService
                //         .QueryMain()
                //         .Where(d => d.MAF_No == request.MAF_No)
                //         .FirstOrDefault();

                var query = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .ToList();

                var employees = _employeeService.QueryEmployee().AsNoTracking()
                   .Where(e => !e.Date_Terminated.HasValue)
                   .Select(e => new { e.ID_No, e.Name })
                   .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var _main = query.ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Endorsed_By,
                        m.Endorser_Remarks,
                        m.DateTime_Endorsed,
                        m.Final_Approver,
                        m.Final_Approver_Remarks,
                        m.DateTime_Approved,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .FirstOrDefault();



                MAFMainDto main = new MAFMainDto();
                if (_main != null)
                {
                    main.MAF_No = _main.MAF_No;
                    main.Endorsed_By = _main.Endorsed_By;
                    main.DateTime_Endorsed = _main.DateTime_Endorsed;
                    main.Endorser_Remarks = _main.Endorser_Remarks;
                    main.Requestor_Name = _main.Requestor_Name;
                    main.Requested_By = _main.Requested_By;
                    main.Status = _main.Status;
                    main.Status_DateTime = _main.Status_DateTime;
                    main.Final_Approver_Remarks = _main.Final_Approver_Remarks;
                    main.Status_Updated_By = _main.Status_Updated_By;
                }


                // send email to the Requestor if there is no left "For Acknowledge MIS or OnHold" request.
                var acknowledge = details.Any(d => d.Status == "For Acknowledgement MIS" || d.Status == "On Hold" || d.Status == "On Going");
                if (!acknowledge)
                {
                    var employee = _employeeService
                                            .QueryEmployee()
                                            .AsNoTracking()
                                            .Where(e => e.Name == existMain.Requestor_Name)
                                            .FirstOrDefault();

                    if (employee != null)
                    {
                        var user = _userService
                                    .QueryUser()
                                    .AsNoTracking()
                                    .Where(e => e.ID_No == employee.ID_No)
                                    .FirstOrDefault();


                        if (user != null)
                        {

                            var _remarks = details.Select(s => s.Status_Remarks).ToList();
                            var attachments = _attachmentsService
                                                .QueryAttachment()
                                                .AsNoTracking()
                                                .Where(a => a.MAF_No == request.MAF_No)
                                                .ToList();

                            var mapPath = Server.MapPath("~/App_Data/Attachments");
                            _emailSenderService.SendEmail(user.Email, "Request Done", main, details, attachments, mapPath, false, "Acknowledge");
                        }
                    }
                }

                return Json(new { success = true, message = "Request update sucessfully.", details, main });
            }
            catch(Exception ex)
            {
                return Json(new {success = false, Message = ex.Message.ToString() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveOnHoldAllAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var userLogin = _userContextService.GetUserLogin();
                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.MAF_No == request.MAF_No && (d.Status == "For Acknowledgement MIS" || d.Status == "On Going"))
                              .ToList();

                if (existDetails == null && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                existMain.Status = "On Going";
                existMain.Status_DateTime = DateTime.Now;
                existMain.Status_Updated_By = userLogin;
                await _mainService.UpdateAsync(existMain);

                // Save to History database

                foreach (var item in existDetails)
                {
                    item.Status = request.Status;
                    item.Status_DateTime = DateTime.Now;
                    item.Status_Updated_By = userLogin;
                    await _detailsService.UpdateAsync(item);
                }

                var details = _detailsService
                               .QueryDetail()
                               .AsNoTracking()
                               .Where(d => d.MAF_No == request.MAF_No)
                               .ToList();

                var updateMain = _mainService
                        .QueryMain()
                        .Where(d => d.MAF_No == request.MAF_No)
                        .FirstOrDefault();

                return Json(new { success = true, message = "Request update sucessfully.", details, main = updateMain });
            }
            catch (Exception ex)
            {
                return Json(new {success = false,  message = ex.Message.ToString()});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveRejectAllAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var userLogin = _userContextService.GetUserLogin();
                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.MAF_No == request.MAF_No && (d.Status == "For Acknowledgement MIS" || d.Status == "On Going" || d.Status == "On Hold"))
                              .ToList();

                if (existDetails.Count == 0 && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                existMain.Status = request.Status;
                existMain.Status_DateTime = DateTime.Now;
                existMain.Status_Updated_By = userLogin;
                await _mainService.UpdateAsync(existMain);

                // Save to History database

                foreach (var item in existDetails)
                {
                    item.Status = request.Status;
                    item.Status_DateTime = DateTime.Now;
                    item.Status_Updated_By = userLogin;
                    await _detailsService.UpdateAsync(item);
                }

                var details = _detailsService
                               .QueryDetail()
                               .AsNoTracking()
                               .Where(d => d.MAF_No == request.MAF_No)
                               .ToList();

                var updateMain = _mainService
                            .QueryMain()
                            .Where(d => d.MAF_No == request.MAF_No)
                            .FirstOrDefault();

                return Json(new { success = true, message = "Request update sucessfully.", details, main = updateMain });
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = ex.Message.ToString() });
            }
        }

        private void LogError(Exception ex)
        {
            // Log the exception with Serilog
            Log.Error(ex, "An error occurred in SaveMisaf");

            // If it's a DbEntityValidationException, log the validation errors
            if (ex is DbEntityValidationException validationException)
            {
                foreach (var error in validationException.EntityValidationErrors)
                {
                    Log.Error("Entity: {EntityType}, State: {EntityState}", error.Entry.Entity.GetType().Name, error.Entry.State);
                    foreach (var validationError in error.ValidationErrors)
                    {
                        Log.Error("Property: {PropertyName}, Error: {ErrorMessage}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
        }
    }
}