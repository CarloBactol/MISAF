using MISAF_Project.Core.Interfaces;
using MISAF_Project.EDMX;
using System;
using System.Data.Entity;

namespace MISAF_Project.Core.Repositories
{
    public class MISRepository : IDisposable
    {
        private readonly DbContext _dbContext;

        public IRepository<MAF_Approver> Approver { get; private set; }
        public IRepository<MAF_Attachment> Attachment { get; private set; }
        public IRepository<MAF_Category> Category { get; private set; }
        public IRepository<MAF_Detail> Detail { get; private set; }
        //public IRepository<MAF_Last_Series> LastSeries { get; private set; }
        public IRepository<MAF_Main> Main { get; private set; }
        public IRepository<MAF_Reason> Reason { get; private set; }

        public MISRepository(MISEntities dbContext)
        {
            _dbContext = dbContext;
            Approver = new EntityRepository<MAF_Approver>(_dbContext);
            Attachment = new EntityRepository<MAF_Attachment>(_dbContext);
            Category = new EntityRepository<MAF_Category>(_dbContext);
            Detail = new EntityRepository<MAF_Detail>(_dbContext);
            Main = new EntityRepository<MAF_Main>(_dbContext);
            Reason = new EntityRepository<MAF_Reason>(_dbContext);
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