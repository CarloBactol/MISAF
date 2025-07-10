using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class HistoryDetailsRepository : BaseRepository<History_MAF_Detail>, IHistoryDetailsRepository
    {
        public HistoryDetailsRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<History_MAF_Detail> QueryHistoryDetail()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(History_MAF_Detail details)
        {
            _dbSet.Add(details);
        }
    }
}