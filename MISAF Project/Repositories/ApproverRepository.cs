using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class ApproverRepository : BaseRepository<MAF_Approver>, IApproverRepository
    {
        public ApproverRepository(MISEntities context) : base(context)
        {
        }

        public new void Add(MAF_Approver approver)
        {
            _dbSet.Add(approver);
        }

        public new void Update(MAF_Approver approver)
        {
            _context.Entry(approver).State = EntityState.Modified;
        }

        public IQueryable<MAF_Approver> QueryApprover()
        {
            return _dbSet.AsQueryable();
        }

    }
}