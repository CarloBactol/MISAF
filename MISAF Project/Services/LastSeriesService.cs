using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public class LastSeriesService : ILastSeriesService
    {
        private readonly ILastSeriesRepository _lastSeriesRepository;

        public LastSeriesService(ILastSeriesRepository lastSeriesRepository)
        {
            _lastSeriesRepository = lastSeriesRepository;
        }

        public async Task AddAsync(MAF_Last_Series lastSeries)
        {
            _lastSeriesRepository.Add(lastSeries);
            await _lastSeriesRepository.SaveChangesAsync();
        }

        public IQueryable<MAF_Last_Series> QueryLastSeries()
        {
            return _lastSeriesRepository.QueryLastSeries();
        }

        public async Task UpdateAsync(MAF_Last_Series lastSeries)
        {
            _lastSeriesRepository.Update(lastSeries);
            await _lastSeriesRepository.SaveChangesAsync();
        }

        public async Task<int> GetNextSeriesAsync(string tableName)
        {
            return await _lastSeriesRepository.GetNextSeriesAsync(tableName);
        }
    }
}