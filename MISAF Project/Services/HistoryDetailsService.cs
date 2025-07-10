using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public class HistoryDetailsService : IHistoryDetailsService
    {
        private readonly IHistoryDetailsRepository _historyDetailsRepository;

        public HistoryDetailsService(IHistoryDetailsRepository historyDetailsRepository)
        {
            _historyDetailsRepository = historyDetailsRepository;
        }

        public async Task AddAsync(History_MAF_Detail detail)
        {
            _historyDetailsRepository.Add(detail);
            await _historyDetailsRepository.SaveChangesAsync(); 
        }

        public IQueryable<History_MAF_Detail> QueryHistoryDetail()
        {
           return _historyDetailsRepository.QueryHistoryDetail();
        }
    }
}