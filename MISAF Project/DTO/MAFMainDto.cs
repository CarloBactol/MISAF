using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.DTO
{
    public class MAFMainDto
    {
        public string MAF_No { get; set; }
        public string Endorsed_By { get; set; }
        public string Endorsed_Status { get; set; }
        public Nullable<DateTime> DateTime_Endorsed { get; set; }
        public Nullable<DateTime> DateTime_Requested { get; set; }
        public Nullable<DateTime> DateTime_Approved { get; set; }
        public string Endorser_Remarks { get; set; }
        public string Requestor_Name { get; set; }
        public string Requestor_ID_No { get; set; }
        public string Requested_By { get; set; }
        public string Requested_By_ID { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> Status_DateTime { get; set; }
        public string Final_Approver { get; set; }
        public string Final_Approver_Remarks { get; set; }
        public string Status_Updated_By { get; set; }
        public string Status_Remarks { get; set; }
        public string Pre_Approved { get; set; }

        public static MAFMainDto CreateFrom(MAF_Main main, List<MAF_Detail> details = null)
        {
            return new MAFMainDto
            {
                MAF_No = main.MAF_No,
                Endorsed_By = main.Endorsed_By,
                DateTime_Endorsed = main.DateTime_Endorsed,
                Endorser_Remarks = main.Endorser_Remarks,
                Requestor_Name = main.Requestor_Name,
                Requested_By = main.Requestor_Name,
                Status = main.Status,
                Status_DateTime = main.Status_DateTime,
                Final_Approver_Remarks = main.Final_Approver_Remarks,
                Status_Updated_By = main.Status_Updated_By,
                Pre_Approved = main.PreApproved,
                DateTime_Approved = main.DateTime_Approved,
                MAF_Details = details ?? new List<MAF_Detail>(),
                Requestor_ID_No = main.Requestor_ID_No
            };
        }

        public List<MAF_Detail> MAF_Details { get; set; }
    }
}