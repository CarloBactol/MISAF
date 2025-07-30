using MISAF_Project.Core.Interfaces;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MISAF_Project.Core.Repositories
{
    public class HRRepository : IDisposable
    {
        private readonly DbContext _dbContext;

        public IRepository<Employee> Employee { get; private set; }

        public HRRepository(HREntities dbContext)
        {
            _dbContext = dbContext;
            Employee = new EntityRepository<Employee>(_dbContext);
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