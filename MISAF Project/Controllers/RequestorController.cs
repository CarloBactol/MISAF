using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using MISAF_Project.EDMX;
using MISAF_Project.Filters;
using MISAF_Project.Services;
using MISAF_Project.Utilities;
using MISAF_Project.ViewModels;
using Serilog;
using AutoMapper;
using AutoMapper.Internal;
using MISAF_Project.DTO;

namespace MISAF_Project.Controllers
{
    [AuthorizedPerson]
    public class RequestorController : BaseController
    {

        private readonly IUserContextService _userContextService;
        private readonly IMainService _mainService;
        private readonly Core.Interfaces.IMainService _mainCoreService;
        private readonly IDetailsService _detailsService;
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IApproverService _approverService;
        private readonly IReasonService _reasonService;
        private readonly ILastSeriesService _lastSeriesService;
        private readonly IAttachmentsService _attachmentsService;
        private readonly IHistoryMainService _historyMain;
        private readonly IHistoryDetailsService _historyDetailsService;
        private readonly IHistoryAttachmentService _historyAttachmentService;

        public RequestorController(IUserContextService userContextService,
            IMainService mainService,
            IDetailsService detailsService,
            IUserService userService,
            IEmployeeService employeeService,
            IEmailSenderService emailSenderService,
            IApproverService approverService,
            IReasonService reasonService,
            ILastSeriesService lastSeriesService,
            IAttachmentsService attachmentsService,
            IHistoryMainService historyMain,
            IHistoryDetailsService historyDetailsService,
            IHistoryAttachmentService historyAttachmentService,
            Core.Interfaces.IMainService mainCoreService)
        {
            _userContextService = userContextService;
            _mainService = mainService;
            _detailsService = detailsService;
            _userService = userService;
            _employeeService = employeeService;
            _emailSenderService = emailSenderService;
            _approverService = approverService;
            _reasonService = reasonService;
            _lastSeriesService = lastSeriesService;
            _attachmentsService = attachmentsService;
            _historyMain = historyMain;
            _historyDetailsService = historyDetailsService;
            _historyAttachmentService = historyAttachmentService;
            _mainCoreService = mainCoreService;
        }



        #region Methods utility

        private List<RequestItem> GetRequestList()
        {
            return Session["Request"] as List<RequestItem> ?? new List<RequestItem>();
        }

        private List<Attachment> GetAttachmentList()
        {
            return Session["Attachment"] as List<Attachment> ?? new List<Attachment>();
        }

        public ActionResult Index()
        {
            ViewBag.Type = "request";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRequestedFor(string name)
        {
            try
            {
                var userLogin = _userContextService.GetUserLogin();

                var employees = _employeeService
                                .QueryEmployee()
                                .AsNoTracking()
                                .Where(x => (x.Name.Contains(name) || x.ID_No == name) && !x.Date_Terminated.HasValue && x.Name != userLogin)
                                .ToList();
                
                if(employees.Count > 0)
                {
                    return Json(new { success = true, employee = employees });
                }

                return Json(new { success = false, message = "No record found." });

            }
            catch (Exception ex)
            {
                return Json(new {success = false, message = ex.Message});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendErrorEmail(string errorMessage, string additionalDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    return Json(new { success = false, message = "Error message cannot be empty." });
                }

                _emailSenderService.SendErrorEmail(errorMessage, additionalDetails);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = $"Failed to send email: {ex.Message}" });
            }
        }

        [HttpGet]
        public JsonResult GetApprovers()
        {
            var endorser = _userContextService.GetEndorser();
            var approver = _userContextService.GetApprover();
            var mis = _userContextService.GetMIS();


            var query = _approverService.QueryApprover().AsNoTracking();

            var approvers = query.Where(x => x.Active == "Y");

            if (endorser != null)
            {
                approvers = approvers.Where(x => x.Name != endorser);
            }
            else if (approver != null)
            {
                approvers = approvers.Where(x => x.Name != approver);
            }
            else if (mis != null)
            {
                approvers = approvers.Where(x => x.Name != mis);
            }


            if (!approvers.Any())
            {
                return Json(new { success = false, message = "No approvers found." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = approvers }, JsonRequestBehavior.AllowGet);
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

        #endregion

        #region Create MISAF
        [HttpGet]
        public JsonResult GetRequestMain()
        {
            try
            {
                var users = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                    UserIDLogin = _userContextService.GetUserIDLogin(),
                };

                //// Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var main = _mainService.QueryMain().AsNoTracking()
                    .Where(m => m.Requestor_Name == users.UserLogin ||
                                (m.Encoded_By != null && m.Encoded_By.StartsWith(users.UserIDLogin + "|") && m.Encoded_By.EndsWith("True")))
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        Requested_For = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();

                //var user = GetAuthUser();
                //var main = _mainCoreService.GetAllByUser(user);

                return Json(new { success = true, main }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CreateRequestor()
        {
            try
            {
                Session["Request"] = null;
                Session["Attachment"] = null;

                var users = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                    UserIDLogin = _userContextService.GetUserIDLogin(),
                    Endorser = _userContextService.GetEndorser(),
                    Approver = _userContextService.GetApprover(),
                    Mis = _userContextService.GetMIS(),
                };


                var approvers = _approverService
                                .QueryApprover()
                                .AsNoTracking()
                                .Where(x => x.Active == "Y");

                if (users.Endorser != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Endorser);
                }
                else if (users.Approver != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Approver);
                }
                else if (users.Mis != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Mis);
                }

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var main = _mainService.QueryMain().AsNoTracking()
                    .Where(m => m.Requestor_Name == users.UserLogin ||
                                (m.Encoded_By != null && m.Encoded_By.StartsWith(users.UserIDLogin + "|") && m.Encoded_By.EndsWith("True")))
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        Requested_For = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();


                return Json(new { success = true, main, users, approvers }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> SaveMisafAsync(MisafViewModel data)
        {
            try
            {
                var mafNo = "";
                var isRequestedFor = false;
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors = modelErrors });
                }

                Employee requestor = null;

                if(data.RequestedFor != null)
                {
                    isRequestedFor = true;
                    requestor = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(e => e.ID_No == data.RequestedFor)
                        .FirstOrDefault();
                }
                else
                {
                    isRequestedFor = false;
                    requestor = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(e => e.Name == data.RequestedBy)
                        .FirstOrDefault();
                }


                var employee = _employeeService
                                         .QueryEmployee()
                                         .AsNoTracking()
                                         .Where(u => u.Name == data.EndorsedNotedBy)
                                         .FirstOrDefault();

                var users = _userService
                                        .QueryUser()
                                        .AsNoTracking()
                                        .Where(u => u.Active == "Y")
                                        .ToList();

                if (requestor == null)
                {
                    return Json(new { success = false, errors = new[] { "Requestor not found." } });
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    int newSeries;
                    try
                    {
                        newSeries = await _lastSeriesService.GetNextSeriesAsync("MAF Main");
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Json(new { success = false, errors = new[] { ex.Message } });
                    }
                    catch (ArgumentException ex)
                    {
                        return Json(new { success = false, errors = new[] { ex.Message } });
                    }

                    // Saving to MAF Main table
                    MAF_Main main = new MAF_Main
                    {
                        MAF_No = MISAFHelper.GenerateNewMISAFNumber(newSeries),
                        Requestor_ID_No = requestor.ID_No,
                        Requestor_Name = requestor.Name,
                        Requestor_Div_Dep = requestor.Division_Code + "-" + requestor.Department_Code,
                        Requestor_Workplace = requestor.Workplace,
                        PreApproved = data.PreApproved ? "Y" : "N",
                        DateTime_Requested = DateTime.Now,
                        Target_Date = data.TargetDate,
                        Status = data.Status,
                        Status_DateTime = DateTime.Now,
                        Status_Updated_By = " ",
                        Endorsed_By = data.EndorsedNotedBy,
                        Endorser_Remarks = null,
                        DateTime_Endorsed = DateTime.Now,
                        Final_Approver = data.FinalApprover,
                        DateTime_Approved = null,
                        Final_Approver_Remarks = null,
                        Encoded_By = MISAFHelper.DeviceInfo() +  isRequestedFor,
                        DateTime_Encoded = DateTime.Now
                    };

                    // Log the MAF_No value before saving
                    Log.Information("Saving MAF_Main with MAF_No: {MAF_No}", main.MAF_No);

                    await _mainService.AddAsync(main);

                    // Saving to MAF Detail table
                    var sessionList = GetRequestList();
                    if (sessionList.Count > 0)
                    {
                        foreach (var session in sessionList)
                        {
                            int newSeriesDetail;
                            try
                            {
                                newSeriesDetail = await _lastSeriesService.GetNextSeriesAsync("MAF Details");
                            }
                            catch (InvalidOperationException ex)
                            {
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }
                            catch (ArgumentException ex)
                            {
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }

                            MAF_Detail detail = new MAF_Detail
                            {
                                Record_ID = newSeriesDetail,
                                MAF_No = main.MAF_No,
                                Category_ID = session.CategoryID,
                                Category = session.Category,
                                Request = session.RequestProblemRecommendation,
                                Reason_ID = session.Reason_ID,
                                Reason = session.Reason,
                                Status = session.Status,
                                Status_DateTime = DateTime.Now,
                                Status_Remarks = null,
                                Status_Updated_By = data.RequestedBy
                            };

                            // Log the MAF_No value before saving
                            Log.Information("Saving MAF_Detail with MAF_No: {MAF_No}", detail.MAF_No);

                            await _detailsService.AddAsync(detail);
                        }
                    }

                    // Saving to MAF Attachment table
                    var sessionAttachmentList = GetAttachmentList();
                    if (sessionAttachmentList.Count > 0)
                    {
                        string attachmentsBasePath = Server.MapPath("~/App_Data/Attachments");
                        string tempUploadsPath = Server.MapPath("~/App_Data/TempUploads/");

                        foreach (var session in sessionAttachmentList)
                        {
                            int newSeriesDetail;
                            try
                            {
                                newSeriesDetail = await _lastSeriesService.GetNextSeriesAsync("MAF Attachments");
                            }
                            catch (InvalidOperationException ex)
                            {
                                Log.Error(ex, "Failed to get series number for MAF Attachments");
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }
                            catch (ArgumentException ex)
                            {
                                Log.Error(ex, "Invalid series number for MAF Attachments");
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }

                            // Construct the temporary and final file paths
                            string tempFilePath = Path.Combine(tempUploadsPath, session.FileName);
                            string uniqueFileName = $"{main.MAF_No}-{newSeriesDetail}-{Path.GetFileName(session.OriginalName)}";
                            string filePath = Path.Combine(attachmentsBasePath, uniqueFileName);

                            // Move the file from the temporary location to the final location
                            try
                            {
                                if (!System.IO.File.Exists(tempFilePath))
                                {
                                    throw new InvalidOperationException($"Temporary file not found for attachment: {session.OriginalName}");
                                }

                                System.IO.File.Move(tempFilePath, filePath);
                                Log.Information("Moved attachment from {TempFilePath} to {FilePath}", tempFilePath, filePath);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to move attachment {OriginalName} from {TempFilePath} to {FilePath}", session.OriginalName, tempFilePath, filePath);
                                return Json(new { success = false, errors = new[] { $"Failed to save attachment {session.OriginalName}: {ex.Message}" } });
                            }

                            MAF_Attachment attach = new MAF_Attachment
                            {
                                Record_ID = newSeriesDetail,
                                MAF_No = main.MAF_No,
                                Filename = session.OriginalName, // Store the original file name
                            };

                            Log.Information("Saving MAF_Attachment with MAF_No: {MAF_No}, FilePath: {FilePath}", attach.MAF_No, filePath);

                            try
                            {
                                await _attachmentsService.AddAsync(attach);
                            }
                            catch (Exception ex)
                            {
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                    Log.Information("Deleted attachment {FilePath} due to database save failure", filePath);
                                    Log.Error(ex, "Saving Field");
                                }
                                throw;
                            }
                        }
                    }

                    // Clean up temporary files after successful save
                    var finalAttachmentList = GetAttachmentList();
                    foreach (var session in finalAttachmentList)
                    {
                        string tempFilePath = Path.Combine(Server.MapPath("~/App_Data/TempUploads/"), session.FileName);
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                            Log.Information("Deleted temporary file {TempFilePath} after successful save", tempFilePath);
                        }
                    }
                    Session["Attachment"] = null;

                    // send email to Final Endorser or Final Approver
                    mafNo = main.MAF_No;
                    scope.Complete();
                }

                if (employee != null)
                {
                    var user = users.FirstOrDefault(u => u.ID_No == employee.ID_No);

                    if (user != null)
                    {
                        //var main1 = _mainService
                        //  .QueryMain()
                        //  .Where(d => d.MAF_No == mafNo)
                        //  .FirstOrDefault();

                        var query = _mainService
                        .QueryMain()
                        .Where(d => d.MAF_No == mafNo)
                        .ToList();

                        var employees1 = _employeeService.QueryEmployee().AsNoTracking()
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
                                    ? employees1.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                                    : null
                            })
                            .FirstOrDefault();



                        MAFMainDto main1 = new MAFMainDto();
                        if (_main != null)
                        {
                            main1.MAF_No = _main.MAF_No;
                            main1.Endorsed_By = _main.Endorsed_By;
                            main1.DateTime_Endorsed = _main.DateTime_Endorsed;
                            main1.Endorser_Remarks = _main.Endorser_Remarks;
                            main1.Requestor_Name = _main.Requestor_Name;
                            main1.Requested_By = _main.Requested_By;
                            main1.Status = _main.Status;
                            main1.Status_DateTime = _main.Status_DateTime;
                            main1.Final_Approver_Remarks = _main.Final_Approver_Remarks;
                            main1.Status_Updated_By = _main.Status_Updated_By;
                        }


                        var details1 = _detailsService
                          .QueryDetail()
                          .Where(d => d.MAF_No == mafNo)
                          .ToList();

                        var attachments = _attachmentsService
                                            .QueryAttachment()
                                            .AsNoTracking()
                                            .Where(a => a.MAF_No == mafNo)
                                            .ToList();
                        var mapPath = Server.MapPath("~/App_Data/Attachments");
                        _emailSenderService.SendEmail(user.Email, "Request For Endorse", main1, details1, attachments, mapPath, false, "Requestor");
                    }

                }

                var encoder = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                    UserIDLogin = _userContextService.GetUserIDLogin(),
                };

                //var query = _mainService.QueryMain().AsNoTracking();
                //var updatedMain = query.Where(m => m.Requestor_Name == encoder.UserLogin).OrderBy(m => m.MAF_No).ToList();

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var updatedMain = _mainService.QueryMain().AsNoTracking()
                    .Where(m => m.Requestor_Name == encoder.UserLogin ||
                                (m.Encoded_By != null && m.Encoded_By.StartsWith(encoder.UserIDLogin + "|") && m.Encoded_By.EndsWith("True")))
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        Requested_For = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();


                return Json(new { success = true, message = "Successfully saved.", main = updatedMain });
            }
            catch (FluentValidation.ValidationException ex)
            {
                // Get controller and action names
                string controllerName = ControllerContext.RouteData.Values["controller"].ToString();
                string actionName = ControllerContext.RouteData.Values["action"].ToString();

                // Log error 
                LogError(ex);

                // Return JSON with controller and action details
                return Json(new
                {
                    success = false,
                    errors = new[] { ex.Message },
                    controller = controllerName,
                    action = actionName
                });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        public JsonResult EditMisafRequest(string index)
        {
            try
            {
                Session["Request"] = null;
                Session["Attachment"] = null;

                var details = _detailsService
                    .QueryDetail()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == index)
                    .ToList();

                var attachments = _attachmentsService
                    .QueryAttachment()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == index)
                    .ToList();

                if (details == null || !details.Any())
                {
                    return Json(new { success = false, message = "No record found." }, JsonRequestBehavior.AllowGet);
                }

                var requestItems = GetRequestList();
                foreach (var item in details)
                {
                    requestItems.Add(new RequestItem
                    {
                        CategoryID = item.Category_ID,
                        Category = item.Category,
                        Reason_ID = item.Reason_ID,
                        Reason = item.Reason,
                        RequestProblemRecommendation = item.Request,
                        Status = item.Status,
                        StatusDate = DateTime.Now,
                    });
                }

                Session["Request"] = requestItems;

                var users = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                    UserIDLogin = _userContextService.GetUserIDLogin(),
                    Endorser = _userContextService.GetEndorser(),
                    Approver = _userContextService.GetApprover(),
                    Mis = _userContextService.GetMIS(),
                };

                var approvers = _approverService
                    .QueryApprover()
                    .AsNoTracking()
                    .Where(x => x.Active == "Y");

                if (users.Endorser != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Endorser);
                }
                else if (users.Approver != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Approver);
                }
                else if (users.Mis != null)
                {
                    approvers = approvers.Where(x => x.Name != users.Mis);
                }

                var categories = new List<CategoryGroup>
                    {
                        new CategoryGroup { CategoryId = 1, GroupName = "Software/Program", Categories = new List<string> { "System Creation", "System Revision", "Report Customization" } },
                        new CategoryGroup { CategoryId = 2, GroupName = "Data", Categories = new List<string> { "Edit" } },
                        new CategoryGroup { CategoryId = 3, GroupName = "Technical", Categories = new List<string> { "Supplies", "Request New Item", "Repair", "Software Installation", "Wifi Connection", "Remote Access" } },
                        new CategoryGroup { CategoryId = 4, GroupName = "Corporate/Plant Manual", Categories = new List<string> { "Upload Manuals", "Request for softcopy of editable manual" } }
                    };

                var reasons = _reasonService.QueryReason().AsNoTracking().Where(r => r.Active == "Y").ToList();

                //var query = _mainService.QueryMain().AsNoTracking();
                //var main = query.Where(m => m.Requestor_Name == users.UserLogin && m.MAF_No == index).OrderBy(m => m.MAF_No).FirstOrDefault();

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var query = _mainService.QueryMain().AsNoTracking().Where(m => m.MAF_No == index).ToList();
                 var main =   query.Where(m => m.Requestor_Name == users.UserLogin ||
                                (m.Encoded_By != null && m.Encoded_By.StartsWith(users.UserIDLogin + "|") && m.Encoded_By.EndsWith("True")))
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Final_Approver,
                        m.Final_Approver_Remarks,
                        m.Endorsed_By,
                        m.DateTime_Endorsed,
                        m.Endorser_Remarks,
                        m.DateTime_Requested,
                        m.DateTime_Approved,
                        m.Requestor_Name,
                        m.Requestor_ID_No,
                        m.PreApproved,
                        m.Target_Date,
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                                ? employees.TryGetValue(m.Encoded_By.Split('|')[0].Trim(), out var name) ? name : null
                                : null,
                        Requested_By_ID = m.Encoded_By != null && m.Encoded_By.Contains("|")
                                ? m.Encoded_By.Split('|')[0].Trim()
                                : null
                    })
                    .FirstOrDefault();



                if (main == null)
                {
                    return Json(new { success = false, message = "No Record found." }, JsonRequestBehavior.AllowGet);
                }

                var sessionList = GetAttachmentList();
                var tempPath = Server.MapPath("~/App_Data/TempUploads/");
                var uploadPath = Server.MapPath("~/App_Data/Attachments/"); 
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                var attachmentListForJson = new List<object>();
                foreach (var file in attachments)
                {
                    var fileName = $"{file.MAF_No}-{file.Record_ID}-{file.Filename}";

                    var originalFilePath = Path.Combine(uploadPath, fileName);
                    if (!System.IO.File.Exists(originalFilePath))
                    {
                        // Skip if the file doesn't exist in the attachments directory
                        continue;
                    }

                    var tempFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Filename);
                    var fullPath = Path.Combine(tempPath, tempFileName);

                    // Copy the file from the upload directory to the temporary directory
                    System.IO.File.Copy(originalFilePath, fullPath, true);

                    var attachment = new Attachment
                    {
                        FileName = tempFileName,
                        OriginalName = file.Filename
                    };

                    sessionList.Add(attachment);

                    // Generate temporary URL for preview
                    var tempUrl = Url.Action("GetTempAttachment", new { fileName = tempFileName });
                    attachmentListForJson.Add(new { attachment.FileName, attachment.OriginalName, TempUrl = tempUrl });
                }

                Session["Attachment"] = sessionList;

                return Json(new
                {
                    success = true,
                    main,
                    users,
                    approvers = approvers.ToList(),
                    details = Session["Request"],
                    attachments = attachmentListForJson,
                    categories,
                    reasons,
                    isEdit = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateMisafAsync(MisafViewModel data)
        {
            try
            {
                var mafNo = "";
                var isRequestedFor = false;
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors = modelErrors });
                }

                var existMain = _mainService
                                .QueryMain()
                                .Where(m => m.MAF_No == data.MafNo)
                                .FirstOrDefault();

                if(existMain == null)
                {
                    return Json(new { success = false, errors = new[] { "MISAF not found." } });
                }

                Employee requestor = null;

                if (data.RequestedFor != null)
                {
                    isRequestedFor = true;
                    requestor = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(e => e.ID_No == data.RequestedFor)
                        .FirstOrDefault();
                }
                else
                {
                    isRequestedFor = false;
                    requestor = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(e => e.Name == data.RequestedBy)
                        .FirstOrDefault();
                }


                var employee = _employeeService
                                         .QueryEmployee()
                                         .AsNoTracking()
                                         .Where(u => u.Name == data.EndorsedNotedBy)
                                         .FirstOrDefault();

                var users = _userService
                                        .QueryUser()
                                        .AsNoTracking()
                                        .Where(u => u.Active == "Y")
                                        .ToList();

                if (requestor == null)
                {
                    return Json(new { success = false, errors = new[] { "Requestor not found." } });
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    int newHistoryMainSeries;
                    int newHistoryDetailsSeries;
                    int newHistoryAttachmenstSeries;
                    try
                    {
                        newHistoryMainSeries = await _lastSeriesService.GetNextSeriesAsync("History MAF Main");
                        newHistoryDetailsSeries = await _lastSeriesService.GetNextSeriesAsync("History MAF Details");
                        newHistoryAttachmenstSeries = await _lastSeriesService.GetNextSeriesAsync("History MAF Attachments");
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Json(new { success = false, errors = new[] { ex.Message } });
                    }
                    catch (ArgumentException ex)
                    {
                        return Json(new { success = false, errors = new[] { ex.Message } });
                    }

                    // Save to History_Main table
                    Mapper.CreateMap<MAF_Main, History_MAF_Main>();  //creating map  
                    var historyMain = Mapper.Map<MAF_Main, History_MAF_Main>(existMain);    
                    historyMain.History_ID = newHistoryMainSeries;
                    await _historyMain.AddAsync(historyMain);

                    existMain.Requestor_ID_No = requestor.ID_No;
                    existMain.Requestor_Name = requestor.Name;
                    existMain.Requestor_Div_Dep = requestor.Division_Code + "-" + requestor.Department_Code;
                    existMain.Requestor_Workplace = requestor.Workplace;
                    existMain.PreApproved = data.PreApproved ? "Y" : "N";
                    existMain.DateTime_Requested = DateTime.Now;
                    existMain.Target_Date = null;
                    existMain.Status = data.Status;
                    existMain.Status_DateTime = DateTime.Now;
                    existMain.Status_Updated_By = " ";
                    existMain.Endorsed_By = data.EndorsedNotedBy;
                    existMain.Endorser_Remarks = null;
                    existMain.DateTime_Endorsed = DateTime.Now;
                    existMain.Final_Approver = data.FinalApprover;
                    existMain.DateTime_Approved = null;
                    existMain.Final_Approver_Remarks = null;
                    existMain.Encoded_By = MISAFHelper.DeviceInfo() + isRequestedFor;
                    existMain.DateTime_Encoded = DateTime.Now;
                    existMain.Target_Date = data.TargetDate;

                    await _mainService.DeleteAsync(existMain); // Remove the MAF with the old keys ex: 25-000004-00

                    var old_MAF_no = existMain.MAF_No;

                    existMain.MAF_No = MISAFHelper.GetNextRevision(existMain.MAF_No);

                    // Log the MAF_No value before saving
                    Log.Information("Saving MAF_Main with MAF_No: {MAF_No}", existMain.MAF_No);

                    await _mainService.AddAsync(existMain);  // Add the Previous MAF with the new keys ex: 25-000004-01 (revision no was incremented)

                    var existDetails = _detailsService.QueryDetail().AsNoTracking().Where(d => d.MAF_No == old_MAF_no).ToList();

                    foreach (var item in existDetails)
                    {
                        try
                        {
                            newHistoryDetailsSeries = await _lastSeriesService.GetNextSeriesAsync("History MAF Details");
                        }
                        catch (InvalidOperationException ex)
                        {
                            return Json(new { success = false, errors = new[] { ex.Message } });
                        }
                        catch (ArgumentException ex)
                        {
                            return Json(new { success = false, errors = new[] { ex.Message } });
                        }


                        // Save to History_Main table
                        Mapper.CreateMap<MAF_Detail, History_MAF_Detail>();  //creating map  
                        var historyDetails = Mapper.Map<MAF_Detail, History_MAF_Detail>(item);
                        historyDetails.History_ID = newHistoryDetailsSeries;
                        await _historyDetailsService.AddAsync(historyDetails);

                        // remove previous details 
                        await _detailsService.DeleteAsync(item);
                    }

                    var sessionList = GetRequestList();
                    if (sessionList.Count > 0)
                    {
                        foreach (var session in sessionList)
                        {
                            int newSeriesDetail;
                            try
                            {
                                newSeriesDetail = await _lastSeriesService.GetNextSeriesAsync("MAF Details");
                            }
                            catch (InvalidOperationException ex)
                            {
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }
                            catch (ArgumentException ex)
                            {
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }

                            MAF_Detail detail = new MAF_Detail
                            {
                                Record_ID = newSeriesDetail,
                                MAF_No = existMain.MAF_No,
                                Category_ID = session.CategoryID,
                                Category = session.Category,
                                Reason_ID = session.Reason_ID,
                                Reason = session.Reason,
                                Request = session.RequestProblemRecommendation,
                                Status = data.Status,
                                Status_DateTime = DateTime.Now,
                                Status_Remarks = null,
                                Status_Updated_By = data.RequestedBy
                            };

                            // Log the MAF_No value before saving
                            Log.Information("Saving MAF_Detail with MAF_No: {MAF_No}", detail.MAF_No);

                            await _detailsService.AddAsync(detail);
                        }
                    }


                    var existAttachments = _attachmentsService.QueryAttachment().AsNoTracking().Where(d => d.MAF_No == existMain.MAF_No).ToList();
                    foreach (var item in existAttachments)
                    {
                        try
                        {
                            newHistoryAttachmenstSeries = await _lastSeriesService.GetNextSeriesAsync("History MAF Attachments");
                        }
                        catch (InvalidOperationException ex)
                        {
                            return Json(new { success = false, errors = new[] { ex.Message } });
                        }
                        catch (ArgumentException ex)
                        {
                            return Json(new { success = false, errors = new[] { ex.Message } });
                        }


                        // Save to History_MAF_Attachment table
                        Mapper.CreateMap<MAF_Attachment, History_MAF_Attachment>();  //creating map  
                        var historyAttachments = Mapper.Map<MAF_Attachment, History_MAF_Attachment>(item);
                        historyAttachments.History_ID = newHistoryAttachmenstSeries;
                        await _historyAttachmentService.AddAsync(historyAttachments);

                        // remove previous details 
                        await _attachmentsService.DeleteAsync(item);
                    }

                    var sessionAttachmentList = GetAttachmentList();
                    if (sessionAttachmentList.Count > 0)
                    {
                        string attachmentsBasePath = Server.MapPath("~/App_Data/Attachments");
                        string tempUploadsPath = Server.MapPath("~/App_Data/TempUploads/");

                        foreach (var session in sessionAttachmentList)
                        {
                            int newSeriesDetail;
                            try
                            {
                                newSeriesDetail = await _lastSeriesService.GetNextSeriesAsync("MAF Attachments");
                            }
                            catch (InvalidOperationException ex)
                            {
                                Log.Error(ex, "Failed to get series number for MAF Attachments");
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }
                            catch (ArgumentException ex)
                            {
                                Log.Error(ex, "Invalid series number for MAF Attachments");
                                return Json(new { success = false, errors = new[] { ex.Message } });
                            }

                            // Construct the temporary and final file paths
                            string tempFilePath = Path.Combine(tempUploadsPath, session.FileName);
                            string uniqueFileName = $"{existMain.MAF_No}-{newSeriesDetail}-{Path.GetFileName(session.OriginalName)}";
                            string filePath = Path.Combine(attachmentsBasePath, uniqueFileName);

                            // Move the file from the temporary location to the final location
                            try
                            {
                                if (!System.IO.File.Exists(tempFilePath))
                                {
                                    throw new InvalidOperationException($"Temporary file not found for attachment: {session.OriginalName}");
                                }

                                // Check if the destination file exists and delete it
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                    Log.Information("Deleted existing file at {FilePath}", filePath);
                                }

                                System.IO.File.Move(tempFilePath, filePath);
                                Log.Information("Moved attachment from {TempFilePath} to {FilePath}", tempFilePath, filePath);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to move attachment {OriginalName} from {TempFilePath} to {FilePath}", session.OriginalName, tempFilePath, filePath);
                                return Json(new { success = false, errors = new[] { $"Failed to save attachment {session.OriginalName}: {ex.Message}" } });
                            }

                            MAF_Attachment attach = new MAF_Attachment
                            {
                                Record_ID = newSeriesDetail,
                                MAF_No = existMain.MAF_No,
                                Filename = session.OriginalName, // Store the original file name
                            };

                            Log.Information("Saving MAF_Attachment with MAF_No: {MAF_No}, FilePath: {FilePath}", attach.MAF_No, filePath);

                            try
                            {
                                await _attachmentsService.AddAsync(attach);
                            }
                            catch (Exception ex)
                            {
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                    Log.Information("Deleted attachment {FilePath} due to database save failure", filePath);
                                    Log.Error(ex, "Saving Field");
                                }
                                throw;
                            }
                        }
                    }

                    // Clean up temporary files after successful save
                    var finalAttachmentList = GetAttachmentList();
                    foreach (var session in finalAttachmentList)
                    {
                        string tempFilePath = Path.Combine(Server.MapPath("~/App_Data/TempUploads/"), session.FileName);
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                            Log.Information("Deleted temporary file {TempFilePath} after successful save", tempFilePath);
                        }
                    }
                    Session["Attachment"] = null;
                    Session["Request"] = null;

                    mafNo = existMain.MAF_No;
                    scope.Complete();
                }

                // send email to Final Endorser or Final Approver
                if (employee != null)
                {
                    var user = users.FirstOrDefault(u => u.ID_No == employee.ID_No);

                    if (user != null)
                    {
                        //var main1 = _mainService
                        //  .QueryMain()
                        //  .Where(d => d.MAF_No == mafNo)
                        //  .FirstOrDefault();

                        var query = _mainService
                         .QueryMain()
                         .Where(d => d.MAF_No == mafNo)
                         .ToList();

                        var employees1 = _employeeService.QueryEmployee().AsNoTracking()
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
                                    ? employees1.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                                    : null
                            })
                            .FirstOrDefault();



                        MAFMainDto main1 = new MAFMainDto();
                        if (_main != null)
                        {
                            main1.MAF_No = _main.MAF_No;
                            main1.Endorsed_By = _main.Endorsed_By;
                            main1.DateTime_Endorsed = _main.DateTime_Endorsed;
                            main1.Endorser_Remarks = _main.Endorser_Remarks;
                            main1.Requestor_Name = _main.Requestor_Name;
                            main1.Requested_By = _main.Requested_By;
                            main1.Status = _main.Status;
                            main1.Status_DateTime = _main.Status_DateTime;
                            main1.Final_Approver_Remarks = _main.Final_Approver_Remarks;
                            main1.Status_Updated_By = _main.Status_Updated_By;
                        }



                        var details1 = _detailsService
                          .QueryDetail()
                          .Where(d => d.MAF_No == mafNo)
                          .ToList();

                        var attachments = _attachmentsService
                                            .QueryAttachment()
                                            .AsNoTracking()
                                            .Where(a => a.MAF_No == mafNo)
                                            .ToList();

                        var mapPath = Server.MapPath("~/App_Data/Attachments");
                        _emailSenderService.SendEmail(user.Email, "Request For Endorse", main1, details1, attachments, mapPath, false, "Requestor");
                    }


                }

                var encoder = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                    UserIDLogin = _userContextService.GetUserIDLogin(),
                };

                // Fetch employee data into memory to avoid EF translation issues
                var employees = _employeeService.QueryEmployee().AsNoTracking()
                    .Where(e => !e.Date_Terminated.HasValue)
                    .Select(e => new { e.ID_No, e.Name })
                    .ToDictionary(e => e.ID_No, e => e.Name);

                // Main query
                var updatedMain = _mainService.QueryMain().AsNoTracking()
                    .Where(m => m.Requestor_Name == encoder.UserLogin ||
                                (m.Encoded_By != null && m.Encoded_By.StartsWith(encoder.UserIDLogin + "|") && m.Encoded_By.EndsWith("True")))
                    .OrderBy(m => m.MAF_No)
                    .ToList() // Materialize to memory to allow Split
                    .Select(m => new
                    {
                        m.MAF_No,
                        m.Status_Remarks,
                        m.Status_DateTime,
                        m.Status_Updated_By,
                        m.Status,
                        m.Requestor_Name,
                        Requested_For = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();

                return Json(new { success = true, message = "Successfully updated.", main = updatedMain });

            }
            catch (FluentValidation.ValidationException ex)
            {
                LogError(ex);
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        #endregion

        #region Request Details CRUD
        [HttpGet]
        public JsonResult CreateRequest()
        {
            try
            {

                var details = GetRequestList();
                var attachments = GetAttachmentList();

                var categories = new List<CategoryGroup>
                    {
                        new CategoryGroup { CategoryId = 1, GroupName = "Software/Program", Categories = new List<string> { "System Creation", "System Revision", "Report Customization" } },
                        new CategoryGroup { CategoryId = 2, GroupName = "Data", Categories = new List<string> { "Edit" } },
                        new CategoryGroup { CategoryId = 3, GroupName = "Technical", Categories = new List<string> { "Supplies", "Request New Item", "Repair", "Software Installation", "Wifi Connection", "Remote Access" } },
                        new CategoryGroup { CategoryId = 4, GroupName = "Corporate/Plant Manual", Categories = new List<string> { "Upload Manuals", "Request for softcopy of editable manual" } }
                    };

                var reasons = _reasonService.QueryReason().AsNoTracking().Where(r => r.Active == "Y").ToList();

                return Json(new { success = true, categories, reasons, details, attachments }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateRequest(RequestItem data)
        {
            try
            {
                var sessionList = GetRequestList();
                var newRequest = new RequestItem();

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();

                    return Json(new { success = false, message = "Validation failed", errors = errors }, JsonRequestBehavior.AllowGet);
                }

                var grouped = new List<CategoryGroup>
                            {
                                new CategoryGroup { CategoryId = 1, GroupName = "Software/Program", Categories = new List<string> { "System Creation", "System Revision", "Report Customization" } },
                                new CategoryGroup { CategoryId = 2, GroupName = "Data", Categories = new List<string> { "Edit" } },
                                new CategoryGroup { CategoryId = 3, GroupName = "Technical", Categories = new List<string> { "Supplies", "Request New Item", "Repair", "Software Installation", "Wifi Connection", "Remote Access" } },
                                new CategoryGroup { CategoryId = 4, GroupName = "Corporate/Plant Manual", Categories = new List<string> { "Upload Manuals", "Request for softcopy of editable manual" } }
                            };

                var category = grouped.Where(temp => temp.Categories.Contains(data.Category)).FirstOrDefault();
                if (category == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                if (data.Index >= 0 && data.Index < sessionList.Count)
                    // kapag nag edit dito mag save yung prev selected session
                    sessionList[data.Index] = new RequestItem
                    {
                        CategoryID = category.CategoryId,
                        Category = data.Category,
                        Reason_ID = data.Reason_ID,
                        Reason = data.Reason,
                        RequestProblemRecommendation = data.RequestProblemRecommendation,
                        Status = data.Status,
                        StatusDate = DateTime.Now
                    };
                else
                    // kapag nag new session ng request
                    sessionList.Add(new RequestItem
                    {
                        CategoryID = category.CategoryId,
                        Category = data.Category,
                        Reason_ID = data.Reason_ID,
                        Reason = data.Reason,
                        RequestProblemRecommendation = data.RequestProblemRecommendation,
                        Status = data.Status,
                        StatusDate = DateTime.Now,
                    });

                Session["Request"] = sessionList;
                var request = GetRequestList();
                return Json(new { success = true, message = "Added", details = request }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult EditRequest(int index)
        {
            try
            {
                var sessionList = GetRequestList();
                if (index >= 0 && index < sessionList.Count)
                {
                    return Json(sessionList[index]);
                }
                return Json(new { success = false, message = $"No record found. Request Id:{index}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteRequest(int index)
        {
            var sessionList = GetRequestList();
            if (index >= 0 && index < sessionList.Count)
                sessionList.RemoveAt(index);
            Session["Request"] = sessionList;
            return Json(new { success = true, message = "Added", details = sessionList }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Attachment CRUD

        [HttpPost]
        public JsonResult CreateAttachment(HttpPostedFileBase file, int index = -1)
        {
            var sessionList = GetAttachmentList();

            if (file != null && file.ContentLength > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
                if (!allowedTypes.Contains(file.ContentType))
                    return Json(new { success = false, message = "Invalid file type." });

                if (file.ContentLength > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "File size exceeds 5MB." });

                if (index == -1 && sessionList.Count >= 5)
                    return Json(new { success = false, message = "Maximum of 5 attachments allowed." });

                if (index == -1 && sessionList.Any(a => a.OriginalName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase)))
                    return Json(new { success = false, message = "An attachment with the same file name already exists." });

                var tempFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var tempPath = Server.MapPath("~/App_Data/TempUploads/");
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                var fullPath = Path.Combine(tempPath, tempFileName);
                file.SaveAs(fullPath);

                var attachment = new Attachment
                {
                    FileName = tempFileName,
                    OriginalName = file.FileName
                };

                if (index >= 0 && index < sessionList.Count)
                {
                    sessionList[index] = attachment;
                }
                else
                {
                    sessionList.Add(attachment);
                }

                Session["Attachment"] = sessionList;

                // Include a temporary URL for preview
                var tempUrl = Url.Action("GetTempAttachment", new { fileName = tempFileName });

                return Json(new { success = true, attachments = sessionList.Select(a => new { a.FileName, a.OriginalName, TempUrl = Url.Action("GetTempAttachment", new { fileName = a.FileName }) }) });
            }

            return Json(new { success = false, message = "No file uploaded." });
        }

        [HttpPost]
        public JsonResult DeleteAttachment(int index)
        {
            var sessionList = GetAttachmentList();

            if (index >= 0 && index < sessionList.Count)
            {
                var fileName = sessionList[index].FileName;
                var filePath = Server.MapPath("~/App_Data/TempUploads/" + fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                sessionList.RemoveAt(index);
                Session["Attachment"] = sessionList;

                return Json(new { success = true, attachments = sessionList });
            }

            return Json(new { success = false, message = "Invalid index." });
        }

      
        #endregion

    }
}