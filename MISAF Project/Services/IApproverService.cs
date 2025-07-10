using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IApproverService
    {
        IQueryable<MAF_Approver> QueryApprover(); // used IQueryable para makuha niya yung latest dbset() | carlob
        void Add(ApproverDto approver);
        void Update(ApproverDto approver);
        void Delete(int id);
        MAF_Approver GetApproverById(int id);
        MAF_Approver GetIdNo(string id);

    }
}
