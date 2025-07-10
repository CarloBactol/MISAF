using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface ILastSeriesRepository : IBaseRepository<MAF_Last_Series>
    {
        IQueryable<MAF_Last_Series> QueryLastSeries();
        Task<int> GetNextSeriesAsync(string tableName);
    }
}
