using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Core.Data
{
    public class MainData
    {
        public string MAF_No { get; set; }

        // VALUE
        // ENCODED BY NAME
        public string Requested_By { get; set; } // ALWAY ENCODED BY NAME

        // VALUE
        // IF REQUESTER ID_NO (NOT EQUAL) ENCODED BY ID_NO, VALUE = REQUESTOR_NAME
        public string Requested_For { get; set; }

        public string Status { get; set; }
        public DateTime Status_Date { get; set; }
        public string Status_Updated_By { get; set; }
        public string Status_Remarks { get; set; }
        public List<string> Requestors_ID { get; set; }

        public bool Is_Owner { get; set; }
        public bool Can_Edit  { get; set; }
        public bool Can_Endorse  { get; set; }
        public bool Can_Approve  { get; set; }
        public bool Can_Acknowledge  { get; set; }
    }
}