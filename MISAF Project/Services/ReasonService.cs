using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Services
{
    public class ReasonService : IReasonService
    {
        private readonly IReasonRepository _reasonRepository;
        
        public ReasonService(IReasonRepository reasonRepository)
        {
            _reasonRepository = reasonRepository;
        }

        public IQueryable<MAF_Reason> QueryReason()
        {
           return _reasonRepository.QueryReason();
        }
    }
}