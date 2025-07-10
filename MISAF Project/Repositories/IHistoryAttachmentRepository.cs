using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public interface IHistoryAttachmentRepository : IBaseRepository<History_MAF_Attachment>
    {
        IQueryable<History_MAF_Attachment> QueryHistoryAttachment();
    }
}