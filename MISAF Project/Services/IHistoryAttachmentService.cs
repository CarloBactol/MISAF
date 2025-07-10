using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public interface IHistoryAttachmentService
    {
        IQueryable<History_MAF_Attachment> QueryHistoryAttachment();

        Task AddAsync(History_MAF_Attachment attachment);
    }
}