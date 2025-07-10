using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class HistoryMainRepository : BaseRepository<History_MAF_Main>, IHistoryMainRepository
    {
        public HistoryMainRepository(MISEntities context) : base(context) { }

        IQueryable<History_MAF_Main> IHistoryMainRepository.QueryHistoryMain()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(History_MAF_Main main)
        {
            _dbSet.Add(main);
        }
    }
}