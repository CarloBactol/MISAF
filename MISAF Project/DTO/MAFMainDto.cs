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
    }
}