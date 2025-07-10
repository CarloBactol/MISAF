using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IMainService
    {
        IQueryable<MAF_Main> QueryMain();
        Task AddAsync(MAF_Main main); // Change to async
        Task UpdateAsync(MAF_Main main); // Change to async
    }
}
