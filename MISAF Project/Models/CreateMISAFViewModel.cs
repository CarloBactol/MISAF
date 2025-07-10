using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace MISAF_Project.Models
{
    public class CreateMISAFViewModel
    {
        public string MAF_No_ { get; set; }

        public string Requestor_ID_No_ { get; set; }

        public string Requestor_Name {  get; set; }

        public string Requestor_Workplace {  get; set; }
        
        public string Requestor_Div_Dep {  get; set; }

        public DateTime DateTime_Requested { get; set; }

        public string Endorsed_By { get; set; }

        public DateTime DateTime_Endorsed { get; set; }

        public string Endorser_Remarks { get; set; }

        public string Final_Approver { get; set; }

        public DateTime DateTime_Approved { get; set; }

        public string Final_Approver_Remarks { get; set; }

        public string PreApproved { get; set; }

        public DateTime Target_Date { get; set; }

        public string Status { get; set; }

        public DateTime Status_DateTime { get; set; }

        public string Status_Updated_By { get; set; }

        public string Status_Remarks { get; set; }

        public string Encoded_By { get; set; }

        public DateTime DateTime_Encoded { get; set; }


        public List<CreateMISAFViewModel_Details> Details_List { get; set; }

        public CreateMISAFViewModel()
        {
            Details_List = new List<CreateMISAFViewModel_Details>();
        }

    }

    public class CreateMISAFViewModel_Details
    {
        public int History_ID { get; set; }
        public int Record_ID { get; set; }
        public string MAF_No_ {  get; set; }
        public int Category_ID { get; set; }
        public string Category { get; set; }
        public string Request { get; set; }
        public string Status { get; set; }
        public DateTime Status_DateTime { get; set; }
        public string Status_Remarks  { get; set; }
        public string Status_Updated_By { get; set; }

        public CreateMISAFViewModel_Details()
        {
            History_ID = 0;
            Record_ID = 0;
            MAF_No_ = string.Empty;
            Category_ID = 0;
            Category = string.Empty;
            Request = string.Empty;
            Status = string.Empty;
            Status_DateTime = DateTime.Now;
            Status_Remarks = string.Empty;
            Status_Updated_By = string.Empty;

        }

    }
}