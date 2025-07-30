using MISAF_Project.Core.Interfaces;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Core.Repositories
{
    public class MasterFileRepository : IDisposable
    {
        private readonly DbContext _dbContext;

        public IRepository<User> User { get; private set; }

        public MasterFileRepository(Master_FilesEntities dbContext)
        {
            _dbContext = dbContext;
            User = new EntityRepository<User>(_dbContext);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async void SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}