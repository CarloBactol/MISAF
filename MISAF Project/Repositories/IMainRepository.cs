using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface IMainRepository : IBaseRepository<MAF_Main>
    {
        IQueryable<MAF_Main> QueryMain();
    }

    
}
