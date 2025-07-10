using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.DTO
{
    public class ApproverDto
    {
        public int Approver_ID { get; set; }
        public string ID_No { get; set; }
        public string Name { get; set; }
        public string Endorser_Only { get; set; }
        public string Email_CC { get; set; }
        public string Active { get; set; }
        public string MIS { get; set; }
    }
}