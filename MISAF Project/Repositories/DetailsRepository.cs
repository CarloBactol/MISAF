using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class DetailsRepository : BaseRepository<MAF_Detail>, IDetailsRepository
    {
        public DetailsRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<MAF_Detail> QueryDetails()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(MAF_Detail detail)
        {
            _dbSet.Add(detail);
        }

        public new void Update(MAF_Detail detail)
        {
            _context.Entry(detail).State = EntityState.Modified;
        }
    }
}