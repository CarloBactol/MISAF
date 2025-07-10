using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(Master_FilesEntities context) : base(context)
        {
        }
      
        public IQueryable<User> QueryUser()
        {
            return _dbSet.AsQueryable();
        }
    }
}