using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface ILastSeriesService
    {
        IQueryable<MAF_Last_Series> QueryLastSeries();
        Task AddAsync(MAF_Last_Series lastSeries); // Change to async
        Task UpdateAsync(MAF_Last_Series lastSeries); // Change to async
        Task<int> GetNextSeriesAsync(string tableName);
    }
}
