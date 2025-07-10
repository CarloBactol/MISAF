using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public class HistoryMainService : IHistoryMainService
    {
        private readonly IHistoryMainRepository _historyMainRepository;

        public HistoryMainService(IHistoryMainRepository historyMainRepository)
        {
            _historyMainRepository = historyMainRepository;
        }

        public async Task AddAsync(History_MAF_Main main)
        {
            _historyMainRepository.Add(main);
            await _historyMainRepository.SaveChangesAsync();
        }

        public IQueryable<History_MAF_Main> QueryHistoryMain()
        {
            return _historyMainRepository.QueryHistoryMain();
        }
    }
}