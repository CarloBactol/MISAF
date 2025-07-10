using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IEmailSenderService
    {
        void SendErrorEmail(string errorMessage, string additionalDetails);
        void SendEmail(string emailApprover, string subject, MAFMainDto main, List<MAF_Detail> details, List<MAF_Attachment> attachments, string MapPath, bool IsAll, string options);
    }
}
