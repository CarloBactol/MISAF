using Antlr.Runtime.Misc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.ViewModels
{
    public class MisafViewModel
    {
        public string __RequestVerificationToken { get; set; }
        public string MafNo { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateRequested { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedFor { get; set; }
        public List<RequestItem> Reasons { get; set; }
        public List<Attachment> Attachments { get; set; }

        public string EndorsedNotedBy { get; set; }
        public string RemarksEndorsedNotedBy { get; set; }
        public string StatusDateEndorsedNotedBy { get; set; }

        public string FinalApprover { get; set; }
        public string RemarksFinalApprover { get; set; }
        public string StatusDateFinalApprover { get; set; }
        public bool PreApproved { get; set; }
        public bool SendEmail { get; set; }
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
    }

    public class RequestItem
    {
        public int Index { get; set; }
        public int CategoryID { get; set; }
        public string Category { get; set; }
        public  string RequestProblemRecommendation { get; set; }
        public List<string> ReasonPurpose { get; set; }
        public int Reason_ID { get; set; }
        public string Reason { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string Remarks { get; set; }
        public string DoneBy { get; set; }
        public string MAF_No { get; set; }
    }

    public class Attachment
    {
        public string FileName { get; set; }
        public string OriginalName { get; set; }
    }

    public class Statuses
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static List<Statuses> GetAll()
        {
            return new List<Statuses>
            {
                new Statuses { Name = "For Approval", Value = "ForApproval" },
                new Statuses { Name = "For Acknowledgment MIS", Value = "ForAcknowledgmentMIS" },
                new Statuses { Name = "On Going", Value = "OnGoing" },
                new Statuses { Name = "Rejected", Value = "Rejected" },
                new Statuses { Name = "On-Hold", Value = "OnHold" },
                new Statuses { Name = "Done", Value = "Done" }
            };
        }

    }



}