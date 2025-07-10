using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using  MISAF_Project.ViewModels;
using System;
using System.Web;
using MISAF_Project.Services;
using System.Data.Entity;
using MISAF_Project.Filters;
using MISAF_Project.Utilities;
using MISAF_Project.EDMX;
using System.Transactions;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Web.UI;
using AutoMapper.Internal;
using MISAF_Project.DTO;

namespace MISAF_Project.Controllers
{
    [AuthorizedPerson]
    public class MISAFController : BaseController
    {
        private static readonly object seriesLock = new object();
        private readonly IReasonService _reasonService;
        private readonly IApproverService _approverService;
        private readonly ILastSeriesService _lastSeriesService;
        private readonly IEmployeeService _employeeService;
        private readonly IMainService _mainService;
        private readonly IDetailsService _detailService;
        private readonly IAttachmentsService _attachmentService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IUserService _userService;
        private readonly IUserContextService _userContextService;

        public MISAFController(IReasonService reasonService, 
            IApproverService approverService, 
            ILastSeriesService lastSeriesService, 
            IEmployeeService employeeService,
            IMainService mainService,
            IDetailsService detailService,
            IAttachmentsService attachmentService,
            IEmailSenderService emailSenderService,
            IUserService userService,
            IUserContextService userContextService)
        {
            _reasonService = reasonService;
            _approverService = approverService;
            _lastSeriesService = lastSeriesService;
            _employeeService = employeeService;
            _mainService = mainService;
            _detailService = detailService;
            _attachmentService = attachmentService;
            _emailSenderService = emailSenderService;
            _userService = userService;
            _userContextService = userContextService;
        }

        private List<RequestItem> GetRequestList()
        {
            return Session["Request"] as List<RequestItem> ?? new List<RequestItem>();
        }

        private List<Attachment> GetAttachmentList()
        {
            return Session["Attachment"] as List<Attachment> ?? new List<Attachment>();
        }

        public JsonResult GetUserRoles()
        {
            var roles = new
            {
                IsMis = _userContextService.GetMIS() != null,
                IsEndorser = _userContextService.GetEndorser() != null,
                IsApprover = _userContextService.GetApprover() != null
            };
            return Json(roles, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Index()
        {
            string ipAddress = GetClientIPv4Address();
            return View();
        }

        [HttpGet]
        public JsonResult GetSavedMisaf()
        {
            try
            {
                var users = new
                {
                    UserLogin = _userContextService.GetUserLogin(),
                };

                var query = _mainService.QueryMain().AsNoTracking();
                var main = query.Where(m => m.Requestor_Name == users.UserLogin).OrderBy(m => m.MAF_No).ToList();
                return Json(new { success = true, main}, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetSavedDetails(string mafNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mafNo))
                {
                    return Json(new { success = false, message = "Invalid MAF number" }, JsonRequestBehavior.AllowGet);
                }
                

                var query = _mainService.QueryMain().AsNoTracking();

                var main = query.Where(m => m.MAF_No == mafNo).OrderBy(m => m.MAF_No).FirstOrDefault();

                if (main == null)
                {
                    return Json(new { success = false, message = "No record found" }, JsonRequestBehavior.AllowGet);
                }

                var detail = _detailService
                    .QueryDetail()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == main.MAF_No)
                    .OrderBy(d => d.MAF_No)
                    .ToList();

                var attachment = _attachmentService
                    .QueryAttachment()
                    .AsNoTracking()
                    .Where(a => a.MAF_No == main.MAF_No)
                    .OrderBy(d => d.MAF_No)
                    .ToList();

                var endorserID = _approverService
                                 .QueryApprover()
                                 .AsNoTracking()
                                 .Where(e => e.Name == main.Endorsed_By)
                                 .Select(e => e.Approver_ID)
                                 .FirstOrDefault();

                var finalApproverID = _approverService
                                 .QueryApprover()
                                 .AsNoTracking()
                                 .Where(e => e.Name == main.Final_Approver)
                                 .Select(e => e.Approver_ID)
                                 .FirstOrDefault();
                var users = new
                {
                    Endorser = endorserID,
                    Approver = finalApproverID,
                };

                return Json(new { success = true, detail, main, attachment, users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetRequestForEditRequestor(int recordID)
        {
            try
            {
                if (recordID == 0)
                {
                    return Json(new { success = false, message = "Invalid Record ID" }, JsonRequestBehavior.AllowGet);
                }


                var request = _detailService
                            .QueryDetail()
                            .AsNoTracking()
                            .Where(d => d.Record_ID == recordID)
                            .FirstOrDefault();

                if (request == null)
                {
                    return Json(new { success = false, message = "No record found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, request }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Mode = "New";
            ViewBag.IsNew = true;
            ViewBag.Statuses = new SelectList(Statuses.GetAll(), "Value", "Name");
            ViewBag.DateNow = DateTime.Now;
            ViewBag.UserLogin = Session["EmployeeName"]?.ToString();
            Session["Request"] = null;
            Session["Attachment"] = null;
            return View();
        }

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
            }else if (approver != null)
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


        private string GetClientIPv4Address()
        {
            try
            {
                // Check for X-Forwarded-For header (proxies/load balancers)
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // Get the first IP in the list
                    string[] addresses = ipAddress.Split(',');
                    ipAddress = addresses[0].Trim();

                    // Validate if it's a valid IPv4 address
                    if (IsIPv4(ipAddress))
                    {
                        return ipAddress;
                    }
                }

                // Fallback to UserHostAddress
                ipAddress = Request.UserHostAddress;

                // Handle localhost IPv6 (::1) case
                if (ipAddress == "::1")
                {
                    return "127.0.0.1"; // Return IPv4 localhost
                }

                // Validate if it's a valid IPv4 address
                if (IsIPv4(ipAddress))
                {
                    return ipAddress;
                }

                // Return null or empty if no valid IPv4 address is found
                return "Unknown";
            }
            catch
            {
                return "Unknown"; // Handle any errors gracefully
            }
        }

        private bool IsIPv4(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            // Try parsing the IP address
            return System.Net.IPAddress.TryParse(ipAddress, out var address) &&
                   address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }


        // ===================================================================================
        //                                      REASON MANAGEMENT
        // ===================================================================================

        [HttpGet]
        public JsonResult GetReasonsByCategory()
        {
            var reasons = _reasonService
                       .QueryReason()
                       .AsNoTracking()
                       .Where(r => r.Active == "Y")
                       .Select(r => new { r.Reason_ID, r.Reason })
                       .ToList();

            return Json(reasons, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGroupedCategories()
        {
            var grouped = new List<CategoryGroup>
            {
                new CategoryGroup { CategoryId = 1, GroupName = "Software/Program", Categories = new List<string> { "System Creation", "System Revision", "Report Customization" } },
                new CategoryGroup { CategoryId = 2, GroupName = "Data", Categories = new List<string> { "Edit" } },
                new CategoryGroup { CategoryId = 3, GroupName = "Technical", Categories = new List<string> { "Supplies", "Request New Item", "Repair", "Software Installation", "Wifi Connection", "Remote Access" } },
                new CategoryGroup { CategoryId = 4, GroupName = "Corporate/Plant Manual", Categories = new List<string> { "Upload Manuals", "Request for softcopy of editable manual" } }
            };

            return Json(grouped, JsonRequestBehavior.AllowGet);
        }


        // ===================================================================================
        //                                      REQUEST MANAGEMENT
        // ===================================================================================

        [HttpGet]
        public JsonResult GetSavedRequest()
        {
            return Json(GetRequestList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddRequest(RequestItem data)
        {
            var sessionList = GetRequestList();
            var newRequest = new RequestItem();

            if (!ModelState.IsValid)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
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

                sessionList[data.Index] = new RequestItem {CategoryID = category.CategoryId, Category = data.Category, 
                                                            ReasonPurpose = data.ReasonPurpose, RequestProblemRecommendation = data.RequestProblemRecommendation, 
                                                            TargetDate = data.TargetDate, Remarks = data.Remarks };
            else
                sessionList.Add(new RequestItem
                {
                    CategoryID = category.CategoryId,
                    Category = data.Category,
                    ReasonPurpose = data.ReasonPurpose,
                    RequestProblemRecommendation = data.RequestProblemRecommendation,
                    TargetDate = data.TargetDate,
                    Remarks = data.Remarks
                });

            Session["Request"] = sessionList;
            return Json(sessionList);
        }

        [HttpPost]
        public JsonResult GetRequestForEdit(int index)
        {
            var sessionList = GetRequestList();
            if (index >= 0 && index < sessionList.Count)
            {
                return Json(sessionList[index]);
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult DeleteRequest(int index)
        {
            var sessionList = GetRequestList();
            if (index >= 0 && index < sessionList.Count)
                sessionList.RemoveAt(index);
            Session["Request"] = sessionList;
            return Json(sessionList);
        }

        // ===================================================================================
        //                            REQUEST MANAGEMENT FOR REQUESTOR          
        // ===================================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateRequest(RequestItem data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var details = _detailService
                    .QueryDetail()
                    .FirstOrDefault(d => d.Record_ID == data.Index);

                if (details == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                var grouped = new List<CategoryGroup>
                    {
                        new CategoryGroup { CategoryId = 1, GroupName = "Software/Program", Categories = new List<string> { "System Creation", "System Revision", "Report Customization" } },
                        new CategoryGroup { CategoryId = 2, GroupName = "Data", Categories = new List<string> { "Edit" } },
                        new CategoryGroup { CategoryId = 3, GroupName = "Technical", Categories = new List<string> { "Supplies", "Request New Item", "Repair", "Software Installation", "Wifi Connection", "Remote Access" } },
                        new CategoryGroup { CategoryId = 4, GroupName = "Corporate/Plant Manual", Categories = new List<string> { "Upload Manuals", "Request for softcopy of editable manual" } }
                    };

                var category = grouped.FirstOrDefault(temp => temp.Categories.Contains(data.Category));
                if (category == null)
                {
                    return Json(new { success = false, message = "Invalid category selected." });
                }

                if (details.Category == data.Category && details.Request == data.RequestProblemRecommendation)
                {
                    return Json(new { success = false, message = "No changes detected." });
                }

                details.Category_ID = category.CategoryId;
                details.Category = data.Category;
                details.Request = data.RequestProblemRecommendation;

                // Save to history database (implement as needed)
                // Example: await _historyService.LogChangeAsync(details);

                // Save to database
                await _detailService.UpdateAsync(details);

                var record = _detailService
                    .QueryDetail()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == details.MAF_No)
                    .ToList();

                return Json(new { success = true, data = record, message = "Request updated successfully." });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred while updating the request." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteRequestEdit(RequestItem data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var details = _detailService
                    .QueryDetail()
                    .FirstOrDefault(d => d.Record_ID == data.Index);

                if (details == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                // Save to history database (implement as needed)
                // Example: await _historyService.LogChangeAsync(details);

                // Delete to database
                await _detailService.DeleteAsync(details);

                var record = _detailService
                    .QueryDetail()
                    .AsNoTracking()
                    .Where(d => d.MAF_No == details.MAF_No)
                    .ToList();

                return Json(new { success = true, data = record, message = "Request deleted successfully." });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = "An error occurred while updating the request." });
            }
        }



        // ===================================================================================
        //                          ATTACHMENT MANAGEMENT            
        // ===================================================================================

        public JsonResult AddAttachment(HttpPostedFileBase file, int index = -1)
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

        // Action to serve temporary files
        public ActionResult GetTempAttachment(string fileName)
        {
            var tempPath = Server.MapPath("~/App_Data/TempUploads/");
            var filePath = Path.Combine(tempPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var originalName = GetAttachmentList().FirstOrDefault(a => a.FileName == fileName)?.OriginalName ?? fileName;
            return File(fileBytes, "application/octet-stream", originalName);
        }

        public JsonResult GetAttachments()
        {
            return Json(GetAttachmentList(), JsonRequestBehavior.AllowGet);
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



        // ===================================================================================
        //                                      CRUD MANAGEMENT
        // ===================================================================================

       


        [FinalApproverAuthFilter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveApproveAsync(string mafNo, int recordId, string remarks)
        {
           
            if(!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                       .SelectMany(v => v.Errors)
                       .Select(e => e.ErrorMessage)
                       .ToList();
                return Json(new { success = false, errors = modelErrors });
            }

            var existMain = _mainService
                            .QueryMain()
                            .AsNoTracking()
                            .Where(temp => temp.MAF_No == mafNo)
                            .FirstOrDefault();
            if (existMain == null)
            {
                return Json(new { success = false, message = $"No main record found \nMAF No:{mafNo}" });
            }

            var existDetail = _detailService
                          .QueryDetail()
                          .AsNoTracking()
                          .Where(d => d.MAF_No == mafNo && d.Record_ID == recordId)
                          .FirstOrDefault();

            if (existDetail == null)
            {
                return Json(new { success = false, message = $"No detail record found \nMAF No:{mafNo} \nRecord Id:{recordId}" });
            }

            var user_id = Session["EmployeeID"].ToString();

            var user = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(emp => emp.ID_No == user_id)
                        .FirstOrDefault();
            if (existMain == null)
            {
                return Json(new { success = false, message = $"No employee record found \nID No:{Session["EmployeeID"]}" });
            }

            var status = Statuses.GetAll().Where(temp => temp.Value == "ForAcknowledgmentMIS").FirstOrDefault();

         

            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    existMain.Status = status.Name;
                    existMain.Status_DateTime = DateTime.Now;
                    existMain.Status_Updated_By = user.Name;
                    existMain.Status_Remarks = remarks;
                    existMain.Final_Approver_Remarks = remarks;
                    existMain.DateTime_Approved = DateTime.Now;
                    Log.Information("Approving MAF_Main with MAF_No: {MAF_No}", existMain.MAF_No);
                    await _mainService.UpdateAsync(existMain);

                    existDetail.Status = status.Name;
                    existDetail.Status_DateTime = DateTime.Now;
                    existDetail.Status_Remarks = remarks;
                    existDetail.Status_Updated_By = user.Name;

                    Log.Information("Approving MAF_Detail with MAF_No: {MAF_No}", existMain.MAF_No);
                    await _detailService.UpdateAsync(existDetail);

                    scope.Complete();
                    return Json(new { success = true, message = "Approved successfully" });
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return Json(new { success = false, errors = new[] { ex.Message } });
                }

            }
        }


        [EndorserAuthFilter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveEndorseAsync(string mafNo, int recordId, string remarks)
        {
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
                            .AsNoTracking()
                            .Where(temp => temp.MAF_No == mafNo)
                            .FirstOrDefault();
            if (existMain == null)
            {
                return Json(new { success = false, message = $"No main record found \nMAF No:{mafNo}" });
            }

            var existDetail = _detailService
                          .QueryDetail()
                          .AsNoTracking()
                          .Where(d => d.MAF_No == mafNo && d.Record_ID == recordId)
                          .FirstOrDefault();

            if (existDetail == null)
            {
                return Json(new { success = false, message = $"No detail record found \nMAF No:{mafNo} \nRecord Id:{recordId}" });
            }

            var user_id = Session["EmployeeID"].ToString();

            var user = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(emp => emp.ID_No == user_id)
                        .FirstOrDefault();
            if (existMain == null)
            {
                return Json(new { success = false, message = $"No employee record found \nID No:{Session["EmployeeID"]}" });
            }

            var status = Statuses.GetAll().Where(temp => temp.Value == "ForAcknowledgmentMIS").FirstOrDefault();



            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    existMain.DateTime_Endorsed = DateTime.Now;
                    existMain.Endorser_Remarks = remarks;
                    existMain.Status_DateTime = DateTime.Now;
                    existMain.Status_Updated_By = user.Name;
                    existMain.Status_Remarks = remarks;
                    existMain.DateTime_Approved = DateTime.Now;
                    Log.Information("Endorsing MAF_Main with MAF_No: {MAF_No}", existMain.MAF_No);
                    await _mainService.UpdateAsync(existMain);

                    existDetail.Status_DateTime = DateTime.Now;
                    existDetail.Status_Remarks = remarks;
                    existDetail.Status_Updated_By = user.Name;

                    Log.Information("Endorsing MAF_Detail with MAF_No: {MAF_No}", existMain.MAF_No);
                    await _detailService.UpdateAsync(existDetail);

                    scope.Complete();
                    return Json(new { success = true, message = "Endorsed successfully" });
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    return Json(new { success = false, errors = new[] { ex.Message } });
                }

            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveMisafAsync(MisafViewModel data)
        {
            try
            {
                var mafNo = "";
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors = modelErrors });
                }

                var requestor = _employeeService
                        .QueryEmployee()
                        .AsNoTracking()
                        .Where(e => e.Name == data.RequestedBy)
                        .FirstOrDefault();

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
                    

                    MAF_Main main = new MAF_Main
                    {
                        MAF_No = MISAFHelper.GenerateNewMISAFNumber(newSeries),
                        Requestor_ID_No = requestor.ID_No,
                        Requestor_Name = requestor.Name,
                        Requestor_Div_Dep = requestor.Division_Code + "-" + requestor.Department_Code,
                        Requestor_Workplace = requestor.Workplace,
                        PreApproved = data.PreApproved ? "Y" : "N",
                        DateTime_Requested = DateTime.Now,
                        Target_Date = null,
                        Status = data.Status,
                        Status_DateTime = DateTime.Now,
                        Status_Updated_By = " ",
                        Endorsed_By = data.EndorsedNotedBy,
                        Endorser_Remarks = null,
                        DateTime_Endorsed = DateTime.Now,
                        Final_Approver = data.FinalApprover,
                        DateTime_Approved = null,
                        Final_Approver_Remarks = null,
                        Encoded_By = data.RequestedBy,
                        DateTime_Encoded = DateTime.Now
                    };

                    // Log the MAF_No value before saving
                    Log.Information("Saving MAF_Main with MAF_No: {MAF_No}", main.MAF_No);

                    await _mainService.AddAsync(main);

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
                                Status = data.Status,
                                Status_DateTime = data.StatusDate,
                                Status_Remarks = null,
                                Status_Updated_By = " "
                            };

                            // Log the MAF_No value before saving
                            Log.Information("Saving MAF_Detail with MAF_No: {MAF_No}", detail.MAF_No);

                            await _detailService.AddAsync(detail);
                        }
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
                                await _attachmentService.AddAsync(attach);
                            }
                            catch (Exception ex)
                            {
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                    Log.Error(ex, ex.Message);
                                    Log.Information("Deleted attachment {FilePath} due to database save failure", filePath);
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


                        var details1 = _detailService
                          .QueryDetail()
                          .Where(d => d.MAF_No == mafNo)
                          .ToList();

                        var attachments = _attachmentService
                                            .QueryAttachment()
                                            .AsNoTracking()
                                            .Where(a => a.MAF_No == mafNo)
                                            .ToList();

                        var mapPath = Server.MapPath("~/App_Data/Attachments");
                        _emailSenderService.SendEmail(user.Email, "Request For Endorse", main1, details1, attachments, mapPath, true, "Endorser");
                    }


                }

                return Json(new { success = true });
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

                // Use the EmailSenderService to send the email
                _emailSenderService.SendErrorEmail(errorMessage, additionalDetails);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                LogError(ex);
                return Json(new { success = false, message = $"Failed to send email: {ex.Message}" });
            }
        }




        /// <summary>
        /// Logs an exception using Serilog, including detailed validation errors if the exception
        /// is a <see cref="DbEntityValidationException"/>.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
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
