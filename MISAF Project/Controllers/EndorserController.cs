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
    [EndorserAuthFilter]
    public class EndorserController : BaseController
    {
        private readonly IUserContextService _userContextService;
        private readonly IMainService _mainService;
        private readonly IDetailsService _detailsService;
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IApproverService _approverService;
        private readonly IAttachmentsService _attachmentsService;

        public EndorserController(IUserContextService userContextService,
            IMainService mainService, 
            IDetailsService detailsService, 
            IUserService userService,
            IEmployeeService employeeService,
            IEmailSenderService emailSenderService,
            IApproverService approverService,
            IAttachmentsService attachmentsService)
        {
            _userContextService = userContextService;
            _mainService = mainService;
            _detailsService = detailsService;
            _userService = userService;
            _employeeService = employeeService;
            _emailSenderService = emailSenderService;
            _approverService = approverService;
            _attachmentsService = attachmentsService;
        }


        // GET: Endorser
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMisafToEndorse()
        {
            try
            {
                var users = new
                {
                    Endorser = _userContextService.GetEndorser(),
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
                        Requested_By = m.Encoded_By != null && m.Encoded_By.Contains("|")
                            ? employees.GetOrDefault(m.Encoded_By.Split('|')[0].Trim())
                            : null
                    })
                    .ToList();

                if (main == null)
                {
                    return Json(new { success = false, message = "No record found" }, JsonRequestBehavior.AllowGet);
                }



                if (users.Endorser != null)
                {
                    main = main.Where(x => x.Status == "For Approval" && x.Endorsed_By == users.Endorser).ToList();
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
        public JsonResult GetEndorseDetails(string mafNo)
        {
            try
            {
                if (string.IsNullOrEmpty(mafNo))
                {
                    return Json(new { success = false, message = "Invalid MAF number" }, JsonRequestBehavior.AllowGet);
                }

                var endorser = _userContextService.GetEndorser();

                var query = _mainService.QueryMain().AsNoTracking();

                if (endorser != null)
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
                        m.Endorser_Remarks,
                        m.DateTime_Endorsed,
                        m.Final_Approver,
                        m.Final_Approver_Remarks,
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
                    Endorser = _userContextService.GetEndorser(),
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
        public async Task<JsonResult> SaveEndorseAsync(RequestItem request)
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

                if (existDetails == null && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
                }

                var isApproved = "N";
                if (request.Status == "Approved")
                {
                    isApproved = "Y";
                }

                var users = _userService
                                  .QueryUser()
                                  .AsNoTracking()
                                  .Where(e => e.Active == "Y")
                                  .ToList();

                var employee = _employeeService
                                               .QueryEmployee()
                                               .AsNoTracking()
                                               .Where(e => e.Name == existMain.Final_Approver)
                                               .FirstOrDefault();

                List<MAF_Detail> _mapperDetails = null;
                var isSave = false;
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var userLogin = _userContextService.GetUserLogin();
                    existDetails.Status = request.Status;
                    existDetails.Status_DateTime = DateTime.Now;
                    existDetails.Status_Remarks = request.Remarks;
                    existDetails.Status_Updated_By = userLogin;
                    await _detailsService.UpdateAsync(existDetails);

                    var details = _detailsService
                              .QueryDetail()
                              .AsNoTracking()
                              .Where(d => d.MAF_No == request.MAF_No)
                              .ToList();

                    var _dremarks = details.Select(s => s.Status_Remarks).ToList();
                    if(_dremarks.Count > 0)
                    {
                        existMain.DateTime_Endorsed = DateTime.Now;
                        existMain.Endorser_Remarks = string.Join(",", _dremarks);
                        await _mainService.UpdateAsync(existMain);
                    }

                    // Define mapping for the individual type
                    Mapper.CreateMap<MAF_Detail, MAF_Detail>();
                    _mapperDetails = Mapper.Map<List<MAF_Detail>>(details);

                    var finalApprover = details.Any(d => d.Status == "For Approval");
                    if (!finalApprover)
                    {
                        if (employee != null)
                        {
                            var _remarks = details.Select(s => s.Status_Remarks).ToList();
                            var isApprove = details.Any(s => s.Status == "Approved");
                            // Update main 
                            existMain.Status = isApprove ? "Approved" : "Disapproved";
                            existMain.Status_DateTime = DateTime.Now;
                            existMain.Status_Updated_By = userLogin;
                            existMain.Status_Remarks = string.Join(",", _remarks);
                            existMain.DateTime_Endorsed = DateTime.Now;
                            existMain.Endorser_Remarks = string.Join(",", _remarks);
                            await _mainService.UpdateAsync(existMain);
                        }
                    }

                    scope.Complete();
                    isSave = true;
                }

                if (isSave)
                {
                    var misUsers = _approverService
                                  .QueryApprover()
                                  .AsNoTracking()
                                  .Where(a => a.MIS == "Y" && a.Name == existMain.Final_Approver)
                                  .Select(a => a.Email_CC)
                                  .ToList();

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
                            m.Requestor_ID_No,
                            m.DateTime_Requested,
                            m.PreApproved,
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
                        main.Endorser_Remarks = _main.Endorser_Remarks;
                        main.Requestor_Name = _main.Requestor_Name;
                        main.Requestor_ID_No = _main.Requestor_ID_No;
                        main.Requested_By = _main.Requested_By;
                        main.DateTime_Requested = _main.DateTime_Requested;
                        main.Requested_By_ID = _main.Requested_By_ID;
                        main.Status = _main.Status;
                        main.Endorsed_Status = isApproved;
                        main.Status_DateTime = _main.Status_DateTime;
                        main.Pre_Approved = _main.PreApproved == "Y" ? "Y" : "N";
                    }

                    if (!String.IsNullOrWhiteSpace(main.Requested_By) && main.Requested_By != main.Requestor_Name)
                    {
                        var email = users.FirstOrDefault(u => u.ID_No == main.Requestor_ID_No && u.Email != null);
                        if (email != null)
                        {
                            misUsers.Add(email.Email);
                        }

                        var email2 = users.FirstOrDefault(u => u.ID_No == main.Requested_By_ID && u.Email != null);
                        if (email2 != null)
                        {
                            misUsers.Add(email2.Email);
                        }
                    }

                    var attachments = _attachmentsService
                                                   .QueryAttachment()
                                                   .AsNoTracking()
                                                   .Where(a => a.MAF_No == request.MAF_No)
                                                   .ToList();

                    var _detail = _mapperDetails.Where(d => d.Record_ID == request.Index).ToList();

                    if(misUsers.Count > 0)
                    {
                        var mapPath = Server.MapPath("~/App_Data/Attachments");
                        if (request.Status == "Approved")
                        {
                            main.Status = request.Status;
                            main.Endorser_Remarks = request.Remarks;
                            _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): FOR APPROVAL", main, _detail, attachments, mapPath, true, "Endorser");
                        }
                        else if (request.Status == "Rejected")
                        {
                            main.Status = request.Status;
                            main.Endorser_Remarks = request.Remarks;
                            _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN REJECTED", main, _detail, attachments, mapPath, true, "Endorser");
                        }

                    }
                }

                return Json(new { success = true, message = "Request updated successfully.", details = _mapperDetails });

            }
            catch(Exception ex)
            {
                return Json(new {success = false, message = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
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


                var existDetails = _detailsService
                              .QueryDetail()
                              .Where(d => d.Status == "For Approval" && d.MAF_No == request.MAF_No)
                              .ToList();

                var users = new { UserLogin = _userContextService.GetUserLogin() };

                if (existDetails.Count > 0 && existMain == null)
                {
                    return Json(new { success = false, message = "No record found." });
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

                MAFMainDto _mapperMainDto = null;

                // Saving with transactions scope
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

                    // send email to the Approver and the requestor
                    var finalApprover = details.Any(d => d.Status == "For Approval");
                    var forApproval = details.Any(d => d.Status == "Approved");
                    if (!finalApprover)
                    {
                        if (employee != null && user != null)
                        {
                            var _remarks = details.Select(s => s.Status_Remarks).ToList();

                            // Update main 
                            existMain.Status = forApproval ? "Approved" : "Rejected";
                            existMain.Status_DateTime = DateTime.Now;
                            existMain.Status_Updated_By = employee.Name;
                            existMain.Status_Remarks = string.Join(",", _remarks);
                            existMain.DateTime_Endorsed = DateTime.Now;
                            existMain.Endorser_Remarks = string.Join(",", _remarks);
                            await _mainService.UpdateAsync(existMain);

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
                                main.Endorsed_Status = forApproval ? "Y" : "N";
                                main.DateTime_Endorsed = _main.DateTime_Endorsed;
                                main.Endorser_Remarks = _main.Endorser_Remarks;
                                main.Final_Approver = _main.Final_Approver;
                                main.DateTime_Approved = _main.DateTime_Approved;
                                main.Final_Approver_Remarks = _main.Final_Approver_Remarks;
                                main.Status = _main.Status;
                                main.Status_DateTime = _main.Status_DateTime;
                                main.Pre_Approved = _main.PreApproved == "Y" ? "Y" : "N";
                            }


                            Mapper.CreateMap<MAFMainDto, MAFMainDto>();
                            _mapperMainDto = Mapper.Map<MAFMainDto, MAFMainDto>(main);

                        }
                    }

                    var updatedDetails = _detailsService
                                           .QueryDetail()
                                           .AsNoTracking()
                                           .Where(d => d.MAF_No == request.MAF_No)
                                           .ToList();

                    var endorserRemarks = updatedDetails.Where(d => d.Status_Updated_By == users.UserLogin)
                                                             .Select(d => d.Status_Remarks)
                                                            .ToList();

                    // saving to main table
                    foreach (var d in updatedDetails)
                    {
                        if (d.Status_Updated_By == users.UserLogin)
                        {
                            existMain.Endorser_Remarks = String.Join(",", endorserRemarks);
                            existMain.DateTime_Endorsed = DateTime.Now;
                            await _mainService.UpdateAsync(existMain);
                        }
                    }
                    scope.Complete();
                }
               
                var attachments = _attachmentsService
                                               .QueryAttachment()
                                               .AsNoTracking()
                                               .Where(a => a.MAF_No == request.MAF_No)
                                               .ToList();


                var misUsers = _approverService
                                           .QueryApprover()
                                           .AsNoTracking()
                                           .Where(a => a.MIS == "Y" && a.Name == _mapperMainDto.Final_Approver)
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

                    var listOfUsers = _userService.QueryUser().AsNoTracking();

                    if(_mapperMainDto.Requestor_ID_No != null)
                    {
                        var rec = listOfUsers.FirstOrDefault(r => r.ID_No == _mapperMainDto.Requestor_ID_No && r.Email != null);
                        if(rec != null)
                        {
                            misUsers.Add(rec.Email);
                        }
                    }

                    if (_mapperMainDto.Requested_By_ID != null)
                    {
                        var rec = listOfUsers.FirstOrDefault(r => r.ID_No == _mapperMainDto.Requested_By_ID && r.Email != null);
                        if (rec != null)
                        {
                            misUsers.Add(rec.Email);
                        }
                    }

                    var mapPath = Server.MapPath("~/App_Data/Attachments");
                    if (request.Status == "Approved")
                    {
                        _mapperMainDto.Status = request.Status;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): MIS ACTION FORM FOR APPROVAL", _mapperMainDto, _det, attachments, mapPath, true, "Endorser");
                    }
                    else if (request.Status == "Rejected")
                    {
                        _mapperMainDto.Status = request.Status;
                        _emailSenderService.SendEmail(String.Join(",", misUsers), $"(MISAF #{request.MAF_No}): YOUR REQUEST HAS BEEN REJECTED", _mapperMainDto, _det, attachments, mapPath, true, "Endorser");
                    }
                }

                return Json(new { success = true, message = "Request updated successfully.", details = getDetails, users });
            }
            catch (Exception ex)
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