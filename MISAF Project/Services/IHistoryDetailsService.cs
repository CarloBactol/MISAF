using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public interface IHistoryDetailsService 
    {
        IQueryable<History_MAF_Detail> QueryHistoryDetail();
        Task AddAsync(History_MAF_Detail detail);
    }
}