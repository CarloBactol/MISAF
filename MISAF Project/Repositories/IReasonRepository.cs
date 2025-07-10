using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface IReasonRepository
    {
        IQueryable<MAF_Reason> QueryReason();
    }
}
