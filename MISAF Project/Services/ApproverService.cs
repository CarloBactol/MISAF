using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MISAF_Project.Services
{
    public class ApproverService : IApproverService
    {
        private readonly IApproverRepository _approverRepository;

        public ApproverService(IApproverRepository approverRepository)
        {
            _approverRepository = approverRepository;
        }

        public void Add(ApproverDto approver)
        {
            if (approver == null)
                throw new ArgumentNullException(nameof(approver));

            // Check if an Approver with the same ID No. already exists
            var existing = GetIdNo(approver.ID_No);
            if (existing != null)
                throw new InvalidOperationException($"Approver with ID No. '{approver.ID_No}' already exists.");

            var mAF_Approver = new MAF_Approver
            {
                Approver_ID = approver.Approver_ID,
                ID_No = approver.ID_No,
                Name = approver.Name,
                Email_CC = approver.Email_CC,
                Endorser_Only = approver.Endorser_Only,
                Active = approver.Active,
                MIS = approver.MIS
            };

            _approverRepository.Add(mAF_Approver);
            _approverRepository.SaveChanges(); 
        }

        public void Delete(int id)
        {
            var existing = _approverRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Approver with Approver ID {id} not found.");

            _approverRepository.Delete(existing.Approver_ID);
            _approverRepository.SaveChanges();
        }

        public IQueryable<MAF_Approver> QueryApprover()
        {
            return _approverRepository.QueryApprover();
        }

      

        public void Update(ApproverDto approver)
        {
            if (approver == null)
                throw new ArgumentNullException(nameof(approver));

            var existingEntity = _approverRepository.GetById(approver.Approver_ID);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Approver with ID {approver.ID_No} not found.");

            existingEntity.Name = approver.Name;
            existingEntity.Email_CC = approver.Email_CC;
            existingEntity.Endorser_Only = approver.Endorser_Only;
            existingEntity.Active = approver.Active;
            existingEntity.MIS = approver.MIS;

            _approverRepository.Update(existingEntity);
            _approverRepository.SaveChanges();
        }

        public MAF_Approver GetApproverById(int id)
        {
          var existing =  _approverRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Approver with Approver ID {id} not found.");
            return existing;
        }

        public MAF_Approver GetIdNo(string id)
        {
            var existing = _approverRepository
                .QueryApprover()
                .AsNoTracking()
                .Where(a => a.ID_No == id)
                .FirstOrDefault();
            return existing;
        }
    }

}