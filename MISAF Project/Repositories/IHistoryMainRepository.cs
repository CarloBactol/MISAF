using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public interface IHistoryMainRepository : IBaseRepository<History_MAF_Main>
    {
        IQueryable<History_MAF_Main> QueryHistoryMain();
    }
}