using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public class HistoryAttachmentService : IHistoryAttachmentService
    {
        private readonly IHistoryAttachmentRepository _historyAttachmentRepository;

        public HistoryAttachmentService(IHistoryAttachmentRepository historyAttachmentRepository)
        {
            _historyAttachmentRepository = historyAttachmentRepository;
        }

        public async Task AddAsync(History_MAF_Attachment attachment)
        {
            _historyAttachmentRepository.Add(attachment);
            await _historyAttachmentRepository.SaveChangesAsync();
        }

        public IQueryable<History_MAF_Attachment> QueryHistoryAttachment()
        {
            return _historyAttachmentRepository.QueryHistoryAttachment();
        }
    }
}