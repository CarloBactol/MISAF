using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class MainRepository : BaseRepository<MAF_Main>, IMainRepository
    {
        public MainRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<MAF_Main> QueryMain()
        {
           return _dbSet.AsQueryable();
        }

        public new void Add(MAF_Main main)
        {
            _dbSet.Add(main);
        }

        public new void Update(MAF_Main main)
        {
            _context.Entry(main).State = EntityState.Modified;
        }
    }
}