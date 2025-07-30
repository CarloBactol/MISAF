using FluentValidation;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Services
{
    public class DetailsService : IDetailsService
    {
        private readonly IDetailsRepository _detailRepository;
        private readonly IValidator<MAF_Detail> _validator;

        public DetailsService(IDetailsRepository detailRepository, IValidator<MAF_Detail> validator)
        {
            _detailRepository = detailRepository;
            _validator = validator;
        }

        public async Task AddAsync(MAF_Detail detail)
        {
            ValidateEntity(detail); 
            _detailRepository.Add(detail);
            await _detailRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(MAF_Detail detail)
        {
            _detailRepository.Delete(detail.Record_ID);
            await _detailRepository.SaveChangesAsync();
        }

        public IQueryable<MAF_Detail> QueryDetail()
        {
           return _detailRepository.QueryDetails();
        }
        

        public async Task UpdateAsync(MAF_Detail detail)
        {
            ValidateEntity(detail);
            _detailRepository.Update(detail);
           await _detailRepository.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<MAF_Detail> details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details), "List of MAF_Detail entity cannot be null.");
            }

            foreach (var detail in details)
            {
                ValidateEntity(detail);
                _detailRepository.Update(detail);
            }

            await _detailRepository.SaveChangesAsync();
        }

        private void ValidateEntity(MAF_Detail detail)
        {
            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail), "MAF_Detail entity cannot be null.");
            }

            var result = _validator.Validate(detail);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new FluentValidation.ValidationException($"Entity validation failed: {string.Join(", ", errors)}");
            }
        }
    }
}