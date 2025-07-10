using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IAttachmentsService
    {
        IQueryable<MAF_Attachment> QueryAttachment();
        Task AddAsync(MAF_Attachment attachment); 
        Task UpdateAsync(MAF_Attachment attachment);
        Task DeleteAsync(MAF_Attachment attachment);
    }
}
