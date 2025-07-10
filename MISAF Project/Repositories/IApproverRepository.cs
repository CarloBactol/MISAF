using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface IApproverRepository : IBaseRepository<MAF_Approver>
    {
        // Note:  used IQueryable para makuha niya yung latest dbset() | carlob
        IQueryable<MAF_Approver> QueryApprover(); 

    }
}
