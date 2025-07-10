using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public interface IHistoryMainService
    {
        IQueryable<History_MAF_Main> QueryHistoryMain();
        Task AddAsync(History_MAF_Main main); 
    }
}