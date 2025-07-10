using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class HistoryAttachmentRepository : BaseRepository<History_MAF_Attachment>, IHistoryAttachmentRepository
    {
        public HistoryAttachmentRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<History_MAF_Attachment> QueryHistoryAttachment()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(History_MAF_Attachment attachment)
        {
            _dbSet.Add(attachment);
        }
    }
}