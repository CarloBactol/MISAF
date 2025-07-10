using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        IQueryable<User> QueryUser();
    }
}