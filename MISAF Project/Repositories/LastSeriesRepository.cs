using MISAF_Project.EDMX;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace MISAF_Project.Repositories
{
    public class LastSeriesRepository : BaseRepository<MAF_Last_Series>, ILastSeriesRepository
    {
        public LastSeriesRepository(MISEntities context) : base(context)
        {
        }

        public IQueryable<MAF_Last_Series> QueryLastSeries()
        {
            return _dbSet.AsQueryable();
        }

        public new void Add(MAF_Last_Series lastSeries)
        {
            _dbSet.Add(lastSeries);
        }

        public new void Update(MAF_Last_Series lastSeries)
        {
            _context.Entry(lastSeries).State = EntityState.Modified;
        }

        public async Task<int> GetNextSeriesAsync(string tableName)
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var lastSeries = await _dbSet.FirstOrDefaultAsync(temp => temp.Table_Name == tableName);
                if (lastSeries == null)
                {
                    throw new InvalidOperationException($"Last series entry for {tableName} not found.");
                }

                int currentSeries = lastSeries?.Series ?? 0;
                if (currentSeries < 0)
                {
                    throw new InvalidOperationException($"Invalid series value for {tableName}: {currentSeries}");
                }

                int newSeries = currentSeries + 1;
                lastSeries.Series = newSeries;

                await _context.SaveChangesAsync();
                scope.Complete();

                return newSeries;
            }
        }
    }
}