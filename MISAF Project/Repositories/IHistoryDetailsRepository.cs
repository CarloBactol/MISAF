using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public interface IHistoryDetailsRepository : IBaseRepository<History_MAF_Detail>
    {
        IQueryable<History_MAF_Detail> QueryHistoryDetail();
    }
}