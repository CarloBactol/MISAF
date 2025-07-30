using MISAF_Project.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MISAF_Project.ViewModels;
using System.Threading.Tasks;
using MISAF_Project.Filters;
using AutoMapper.Internal;
using MISAF_Project.EDMX;
using MISAF_Project.DTO;
using AutoMapper;
using System.Transactions;
using System.Web.Services.Description;
using System.Net.Mail;
using System.Web.Management;

namespace MISAF_Project.Controllers
{
    [FinalApproverAuthFilter]
    public class FinalApproverController : BaseController
    {
        private readonly IUserContextService _userContextService;
        private readonly IMainService _mainService;
        private readonly IDetailsService _detailsService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;
        private readonly IApproverService _approverService;
        private readonly IAttachmentsService _attachmentsService;

        public FinalApproverController(IUserContextService userContextService, 
            IMainService mainService, 
            IDetailsService detailsService,
            IEmailSenderService emailSenderService,
            IEmployeeService employeeService,
            IUserService userService,
            IApproverService approverService,
            IAttachmentsService attachmentsService)
        {
            _userContextService = userContextService;
            _mainService = mainService;
            _detailsService = detailsService;
            _emailSenderService = emailSenderService;
            _employeeService = employeeService;
            _userService = userService;
            _approverService = approverService;
            _attachmentsService = attachmentsService;
        }


        // GET: ApproverManagement
        public ActionResult Index()
        {
            ViewBag.Type = "approve";
            return View();
        }


        [HttpGet]
        public JsonResult GetRequestMain()
        {
            try
            {
                var users = new
                {
                    FinalApprover = _userContextService.GetApprover(),
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
                        m.Target_Date,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();


                if (main == null)
                {
                    return Json(new { success = false, message = "No record found." }, JsonRequestBehavior.AllowGet);
                }

                var hasEndorser = main.Any(m => m.Endorsed_By == null);

                if (users.FinalApprover != null && hasEndorser)
                {
                    main = main.Where(x => (x.Status == "Approved" || x.Status == "For Approval") && x.Final_Approver == users.FinalApprover).ToList();
                }

                if (users.FinalApprover != null && !hasEndorser)
                {
                    main = main.Where(x => (x.Status == "Approved") && x.Final_Approver == users.FinalApprover).ToList();
                }

                return Json(new { success = true, main, users }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetRequestDetails(string mafNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mafNo))
                {
                    return Json(new { success = false, message = "Invalid MAF number." }, JsonRequestBehavior.AllowGet);
                }

                var query = _mainService.QueryMain().AsNoTracking().Where(m => m.MAF_No == mafNo);
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
                        m.Endorser_Remarks,
                        m.DateTime_Endorsed,
                        m.Final_Approver,
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
                    return Json(new { success = false, message = "No record found." }, JsonRequestBehavior.AllowGet);
                }

                var user = new
                {
                    UserLogin = _userContextService.GetUserLogin()
                };

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

                return Json(new { success = true, detail, main, attachment, user }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred." }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveFinalApproverAsync(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Inavalid data." });
                }

                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.Record_ID == request.Index && d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                var usersLogin = new { UserLogin = _userContextService.GetUserLogin() };

                if (existDetails == null && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                var employee = _employeeService
                                           .QueryEmployee()
                                           .AsNoTracking()
                                           .Where(e => e.Name == existMain.Final_Approver)
                                           .FirstOrDefault();

                var users = _userService
                                   .QueryUser()
                                   .AsNoTracking()
                                   .Where(e => e.Active == "Y")
                                   .ToList();

                var employees = _employeeService.QueryEmployee().AsNoTracking()
                       .Where(e => !e.Date_Terminated.HasValue)
                       .Select(e => new { e.ID_No, e.Name })
                       .ToDictionary(e => e.ID_No, e => e.Name);

                // check if the endorser approve the request
                var isApproved = "N";
                if (!String.IsNullOrWhiteSpace(existMain.Endorsed_By))
                {
                    var dt = existDetails.Status == "Approved";
                    if (dt)
                    {
                        isApproved = "Y";
                    }
                }
                else
                {
                    var dt = existDetails.Status == "For Approval";
                    if (dt)
                    {
                        isApproved = "N";
                    }
                }
                bool done = false;  
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    existDetails.Status = request.Status;
                    existDetails.Status_DateTime = DateTime.Now;
                    existDetails.Status_Remarks = request.Remarks;
                    existDetails.Status_Updated_By = _userContextService.GetUserLogin();
                    await _detailsService.UpdateAsync(existDetails);

                    var updatedDetails = _detailsService
                                     .QueryDetail()
                                     .AsNoTracking()
                                     .Where(d => d.MAF_No == request.MAF_No)
                                     .ToList();

                    var finalApproverRemarks = updatedDetails.Where(d => d.Status_Updated_By == usersLogin.UserLogin)
                                                             .Select(d => d.Status_Remarks)
                                                            .ToList();

                    foreach (var d in updatedDetails)
                    {
                        if (d.Status_Updated_By == usersLogin.UserLogin)
                        {
                            existMain.Final_Approver_Remarks = String.Join(",", finalApproverRemarks);
                            existMain.DateTime_Approved = DateTime.Now;
                            await _mainService.UpdateAsync(existMain);
                        }
                    }


                    // send email to the MIS if For Acknowledgement MIS request.
                    var finalApprover = updatedDetails.Any(d => d.Status == "Approved" || d.Status == "For Approval");
                    if (!finalApprover)
                    {
                        done = true;
                        if (employee != null)
                        {
                            var _remarks = updatedDetails.Select(s => s.Status_Remarks).ToList();
                            var isApprove = updatedDetails.Any(s => s.Status == "For Acknowledgement MIS");
                            // Update main 
                            existMain.Status = isApprove ? "For Acknowledgement MIS" : "Rejected";
                            existMain.Status_DateTime = DateTime.Now;
                            existMain.Status_Updated_By = employee.Name;
                            existMain.Status_Remarks = string.Join(",", _remarks);
                            existMain.DateTime_Approved = DateTime.Now;
                            existMain.Final_Approver_Remarks = string.Join(",", _remarks);
                            await _mainService.UpdateAsync(existMain);
                        }
                    }
                    scope.Complete();
                }

                var details = _detailsService
                                   .QueryDetail()
                                   .AsNoTracking()
                                   .Where(d => d.MAF_No == request.MAF_No)
                                   .ToList();


                var query = _mainService
                          .QueryMain()
                          .Where(d => d.MAF_No == request.MAF_No)
                          .ToList();

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
                        m.PreApproved,
                        m.Target_Date,
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
                    main.Requested_By = _main.Requested_By;
                    main.Requested_By_ID = _main.Requested_By_ID;
                    main.Requestor_Name = _main.Requestor_Name;
                    main.Requestor_ID_No = _main.Requestor_ID_No;
                    main.DateTime_Requested = _main.DateTime_Requested;
                    main.Endorsed_By = _main.Endorsed_By;
                    main.Endorsed_Status = isApproved;
                    main.DateTime_Endorsed = _main.DateTime_Endorsed;
                    main.Endorser_Remarks = _main.Endorser_Remarks;
                    main.Final_Approver = _main.Final_Approver;
                    main.DateTime_Approved = _main.DateTime_Approved;
                    main.Final_Approver_Remarks = request.Remarks;
                    main.Status = request.Status;
                    main.Status_DateTime = _main.Status_DateTime;
                    main.Pre_Approved = _main.PreApproved == "Y" ? "Y" : "N";
                }

                var misUsers = _approverService
                                .QueryApprover()
                                .AsNoTracking()
                                .Where(a => a.MIS == "Y")
                                .Select(a => a.Email_CC)
                                .ToList();

                var attachments = _attachmentsService
                                           .QueryAttachment()
                                           .AsNoTracking()
                                           .Where(a => a.MAF_No == request.MAF_No)
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
               

                if (misUsers.Count > 0)
                {
                    var mapPath = Server.MapPath("~/App_Data/Attachments");
                    var _detail = details.Where(d => d.Record_ID == request.Index).ToList();
                    if (request.Status == "For Acknowledgement MIS")
                    {
                        main.Status = request.Status;
                        main.Final_Approver_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): FOR ACKNOWLEDGEMENT", main, _detail, attachments, mapPath, false, "Approver");
                    }
                    else if (request.Status == "Rejected")
                    {
                        main.Status = request.Status;
                        main.Final_Approver_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN REJECTED", main, _detail, attachments, mapPath, false, "Approver");
                    }
                
                   
                }

                return Json(new { success = true, message = "Request updated successfully.", details, users = usersLogin, done, main = _main });
            }
            catch (Exception ex)
            {
                return Json(new {success = true, message = ex.Message.ToString()}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostSaveAll(RequestItem request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                var existMain = _mainService
                              .QueryMain()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .FirstOrDefault();

                // if empty endorser

                // check final approver then filter by For Approval 

                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => (d.Status == "For Approval" | d.Status == "Approved") && d.MAF_No == request.MAF_No)
                              .ToList();

                var users = new { UserLogin = _userContextService.GetUserLogin() };

                if (existDetails.Count > 0 && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                // check if the endorser approve the request
                var isApproved = "N";
                if (!String.IsNullOrWhiteSpace(existMain.Endorsed_By))
                {
                    var dt = existDetails.Any(d => d.Status == "Approved");
                    if (dt)
                    {
                        isApproved = "Y";
                    }
                }
                else
                {
                    var dt = existDetails.Any(d => d.Status == "For Approval");
                    if (dt)
                    {
                        isApproved = "N";
                    }
                }

                // get the details of final approver
                var employee = _employeeService
                                        .QueryEmployee()
                                        .AsNoTracking()
                                        .Where(e => e.Name == existMain.Final_Approver)
                                        .FirstOrDefault();

                var employees = _employeeService.QueryEmployee().AsNoTracking()
                               .Where(e => !e.Date_Terminated.HasValue)
                               .Select(e => new { e.ID_No, e.Name })
                               .ToDictionary(e => e.ID_No, e => e.Name);

                // get the email of final approver
                var user = _userService
                            .QueryUser()
                            .AsNoTracking()
                            .Where(e => e.ID_No == employee.ID_No)
                            .FirstOrDefault();

                List<MAF_Detail> _mapperDetails = null;
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var item in existDetails)
                    {
                        item.Status = request.Status;
                        item.Status_DateTime = DateTime.Now;
                        item.Status_Remarks = request.Remarks;
                        item.Status_Updated_By = _userContextService.GetUserLogin();
                        await _detailsService.UpdateAsync(item);
                    }

                    var details = _detailsService
                              .QueryDetail()
                              .AsNoTracking()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .ToList();

                    Mapper.CreateMap<MAF_Detail, MAF_Detail>();
                    _mapperDetails = Mapper.Map<List<MAF_Detail>>(details);

                    // send email to the MIS and the requestor, if For Acknowledgement MIS
                    var finalApprover = details.Any(d => d.Status == "Approved" || d.Status == "For Approval");
                    var forAcknowledge = details.Any(d => d.Status == "For Acknowledgement MIS");
                    if (!finalApprover)
                    {
                        if (employee != null && user != null)
                        {
                            var _remarks = details.Select(s => s.Status_Remarks).ToList();

                            // Update main 
                            existMain.Status = forAcknowledge ? "For Acknowledgement MIS" : "Rejected";
                            existMain.Status_DateTime = DateTime.Now;
                            existMain.Status_Updated_By = employee.Name;
                            existMain.Status_Remarks = string.Join(",", _remarks);
                            existMain.DateTime_Approved = DateTime.Now;
                            existMain.Final_Approver_Remarks = string.Join(",", _remarks);
                            await _mainService.UpdateAsync(existMain);
                        }
                    }

                    var finalApproverRemarks = _mapperDetails.Where(d => d.Status_Updated_By == users.UserLogin)
                                                             .Select(d => d.Status_Remarks)
                                                            .ToList();

                    // saving to main table
                    foreach (var d in _mapperDetails)
                    {
                        if (d.Status_Updated_By == users.UserLogin)
                        {
                            existMain.Final_Approver_Remarks = String.Join(",", finalApproverRemarks);
                            existMain.DateTime_Approved = DateTime.Now;
                            await _mainService.UpdateAsync(existMain);
                        }
                    }

                    scope.Complete();
                }


                var query = _mainService
                             .QueryMain()
                             .Where(d => d.MAF_No == request.MAF_No)
                             .ToList();

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
                        m.PreApproved,
                        m.Target_Date,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .FirstOrDefault();



                MAFMainDto main = new MAFMainDto();
                if (_main != null)
                {
                    main.MAF_No = _main.MAF_No;
                    main.Requested_By = _main.Requested_By;
                    main.Requestor_Name = _main.Requestor_Name;
                    main.DateTime_Requested = _main.DateTime_Requested;
                    main.Endorsed_By = _main.Endorsed_By;
                    main.Endorsed_Status = isApproved;
                    main.DateTime_Endorsed = _main.DateTime_Endorsed;
                    main.Endorser_Remarks = _main.Endorser_Remarks;
                    main.Final_Approver = _main.Final_Approver;
                    main.DateTime_Approved = _main.DateTime_Approved;
                    main.Final_Approver_Remarks = _main.Final_Approver_Remarks;
                    main.Status = _main.Status;
                    main.Status_DateTime = _main.Status_DateTime;
                    main.Pre_Approved = _main.PreApproved == "Y" ? "Y" : "N";
                }

                var attachments = _attachmentsService
                                               .QueryAttachment()
                                               .AsNoTracking()
                                               .Where(a => a.MAF_No == request.MAF_No)
                                               .ToList();

                var misUsers = _approverService
                                           .QueryApprover()
                                           .AsNoTracking()
                                           .Where(a => a.MIS == "Y")
                                           .Select(a => a.Email_CC)
                                           .ToList();

                var getDetails = _detailsService
                              .QueryDetail()
                              .AsNoTracking()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .ToList();

                var _det = getDetails.Where(d => existDetails.Any(x => x.Record_ID == d.Record_ID)).ToList();

                if (misUsers.Count > 0)
                {

                    var usersService = _userService.QueryUser().AsNoTracking();

                    var requestedFor  = usersService.Where(u => u.ID_No == main.Requestor_ID_No && u.Email != null).FirstOrDefault();
                    if (requestedFor != null)
                    {
                        misUsers.Add(requestedFor.Email);
                    }

                    var requestedBy = usersService.Where(u => u.ID_No == main.Requested_By_ID && u.Email != null).FirstOrDefault();
                    if (requestedBy != null)
                    {
                        misUsers.Add(requestedBy.Email);
                    }

                    var mapPath = Server.MapPath("~/App_Data/Attachments");
                    if(request.Status == "For Acknowledgement MIS")
                    {
                        main.Status = request.Status;
                        main.Final_Approver_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): FOR ACKNOWLEDGEMENT", main, _det, attachments, mapPath, true, "Approver");

                    }else if(request.Status == "Rejected")
                    {
                        main.Status = request.Status;
                        main.Final_Approver_Remarks = request.Remarks;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN REJECTED", main, _det, attachments, mapPath, true, "Approver");
                    }
                }

                return Json(new { success = true, message = "Request updated successfully.", details = getDetails, users, main = _main });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message.ToString() });
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