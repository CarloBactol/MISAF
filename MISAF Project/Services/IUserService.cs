using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public interface IUserService
    {
        IQueryable<User> QueryUser();
    }
}
