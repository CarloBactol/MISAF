using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IDetailsService
    {
        IQueryable<MAF_Detail> QueryDetail();
        Task AddAsync(MAF_Detail detail); 
        Task UpdateAsync (MAF_Detail detail);
        Task DeleteAsync (MAF_Detail detail);

        Task UpdateRangeAsync(List<MAF_Detail> details);
    }
}
