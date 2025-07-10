using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    // ReasonRepository inherits from BaseRepository<MAF_Reason> to reuse common CRUD logic,
    // and implements IReasonRepository for custom functionality related to MAF_Reason.
    public class ReasonRepository : BaseRepository<MAF_Reason>, IReasonRepository
    {
        // Constructor that accepts a DbContext instance (MISEntities) and passes it to the base class constructor.
        // This allows BaseRepository to work with the MAF_Reason DbSet.
        public ReasonRepository(MISEntities context) : base(context)
        {
        }

        // This method returns an IQueryable<MAF_Reason> for querying MAF_Reason entities.
        // _dbSet is assumed to be a protected member from BaseRepository representing DbSet<MAF_Reason>.
        public IQueryable<MAF_Reason> QueryReason()
        {
            return _dbSet.AsQueryable(); // Allows further LINQ operations outside this method
        }
        
    }

}