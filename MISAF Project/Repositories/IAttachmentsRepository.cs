using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface IAttachmentsRepository : IBaseRepository<MAF_Attachment>
    {
        IQueryable<MAF_Attachment> QueryAttachment();
    }
}
