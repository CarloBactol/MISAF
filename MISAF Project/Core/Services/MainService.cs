using MISAF_Project.Core.Data;
using MISAF_Project.Core.Interfaces;
using MISAF_Project.Core.Repositories;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Core.Services
{
    public class MainService : IMainService
    {
        public MISRepository _misRepository { get; private set; }
        public HRRepository _hrRepository { get; private set; }


        public MainService(MISRepository misRepository, HRRepository hrRepository) {
            _misRepository = misRepository;
            _hrRepository = hrRepository;
        }

        public List<MainData> GetAllByUser(UserData user, string type = "")
        {
            List<MainData> result = new List<MainData>();

            var employees = _hrRepository.Employee
                .Query()
                .Where(e => !e.Date_Terminated.HasValue)
                .Select(e => new { e.ID_No, e.Name })
                .AsNoTracking()
                .ToList();

            var query = _misRepository.Main.Query();

            type = type.Trim().ToLower();

            switch (type)
            {
                case "acknowledge":
                    query = query.Where(x => (x.Status == "For Acknowledgement MIS" || x.Status == "On Going" || x.Status == "On Hold") && !x.Encoded_By.Contains(user.IdNo));
                    break;
                case "approve":
                    query = query.Where(x => (x.Status == "Approved" || x.Status == "For Approval") && x.Final_Approver == user.FullName);
                    break;
                case "endorse":
                    query = query.Where(m => m.Status == "For Approval" && m.Endorsed_By == user.FullName);
                    break;
                default:
                    query = query.Where((m) => m.Requestor_ID_No == user.IdNo || m.Encoded_By.StartsWith(user.IdNo + "|"));
                    break;
            }

            var mains = query.AsNoTracking().ToList();

            foreach (var m in mains)
            {
                var encodedByParts = m.Encoded_By.Split('|');
                var encoderIdNo = encodedByParts[0];
                var hasRequestorFor = encodedByParts[2];
                var encoder = employees.Where(e => e.ID_No == encoderIdNo).FirstOrDefault();
                var requestorsIds = new List<string> { encoderIdNo };
                var isOwner = requestorsIds.Contains(user.IdNo);
                var endorser = employees.Where(e => e.Name == user.FullName).FirstOrDefault();

                if (hasRequestorFor == "True")
                {
                    requestorsIds.Add(m.Requestor_ID_No);
                }

                result.Add(new MainData
                {
                    MAF_No = m.MAF_No,
                    Requested_By = encoder?.Name,
                    Requested_For = m.Requestor_Name,
                    Status = m.Status,
                    Status_Date = m.Status_DateTime,
                    Status_Remarks = m.Status_Remarks,
                    Status_Updated_By = m.Status_Updated_By,
                    Requestors_ID = requestorsIds,
                    Is_Owner = isOwner,
                    Can_Edit = (encoderIdNo == user.IdNo) & (m.Status != "Done" & m.Status != "Rejected") & type == "request",
                    Can_Endorse = !isOwner & (m.Endorsed_By == endorser.Name) & type == "endorse",
                    Can_Approve = !isOwner & (m.Final_Approver == endorser.Name) & type == "approve",
                    Can_Acknowledge = !isOwner & user.IsMIS & type == "acknowledge",
                });
            }

            return result;
        }

        public List<MainData> GetAll()
        {
            List<MainData> result = new List<MainData>();

            var employees = _hrRepository.Employee
                .Query()
                .Where(e => !e.Date_Terminated.HasValue)
                .Select(e => new { e.ID_No, e.Name })
                .AsNoTracking()
                .ToList();

            return result;
        }
    }
}