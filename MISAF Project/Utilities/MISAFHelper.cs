using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Utilities
{
    public static class MISAFHelper
    {
        public static string GenerateNewMISAFNumber(int lastSeries)
        {
            string year = DateTime.Now.ToString("yy");
            string paddedSeries = lastSeries.ToString("D6");
            string revision = "00"; // First version always 00
            return $"{year}-{paddedSeries}-{revision}";
        }

        public static string GetNextRevision(string existingMISAFNumber)
        {
            if (string.IsNullOrEmpty(existingMISAFNumber))
                throw new ArgumentException("Existing MISAF number is required.");

            var parts = existingMISAFNumber.Split('-');
            if (parts.Length != 3)
                throw new FormatException("Invalid MISAF number format.");

            int revision = int.Parse(parts[2]);
            revision++;
            return $"{parts[0]}-{parts[1]}-{revision:D2}";
        }

        public static string DeviceInfo()
        {
            HttpContext context = HttpContext.Current;
            string rtn_value = "";
            //rtn_value += context.Request.ServerVariables["AUTH_USER"] + " | "; // AUTHENTICATED USER (LOGIN)
            //rtn_value += context.Request.ServerVariables["LOGON_USER"] + " | "; // WINDOWS LOGIN
           // rtn_value += HttpContext.Current.Session["EmployeeName"] + " | "; // WINDOWS LOGIN
            rtn_value += HttpContext.Current.Session["EmployeeID"] + "|"; // WINDOWS LOGIN
            rtn_value += context.Request.ServerVariables["REMOTE_ADDR"] + "|"; // IP_ADDRESS
                                                                                 //rtn_value += context.Request.ServerVariables["HTTP_USER_AGENT"] + " | "; // BROWSER NAME/TYPE

            if (!string.IsNullOrEmpty(rtn_value))
            {
                if (rtn_value.Length > 100)
                {
                    rtn_value = rtn_value.Substring(0, 100);
                }
            }

            return rtn_value;
        }
    }
}