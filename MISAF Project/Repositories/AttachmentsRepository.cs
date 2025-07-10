using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class AttachmentsRepository : BaseRepository<MAF_Attachment>, IAttachmentsRepository
    {
        public AttachmentsRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<MAF_Attachment> QueryAttachment()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(MAF_Attachment attachment)
        {
            _dbSet.Add(attachment);
        }

        public new void Update(MAF_Attachment attachment)
        {
            _context.Entry(attachment).State = EntityState.Modified;
        }
    }
}