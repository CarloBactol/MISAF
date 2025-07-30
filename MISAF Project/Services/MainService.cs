using FluentValidation;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MISAF_Project.Services
{
    public class MainService : IMainService
    {
        private readonly IMainRepository _mainRepository;
        private readonly IValidator<MAF_Main> _validator;

        public MainService(IMainRepository mainRepository, IValidator<MAF_Main> validator)
        {
            _mainRepository = mainRepository;
            _validator = validator;
        }

        public async Task AddAsync(MAF_Main main)
        {
            ValidateEntity(main);
            _mainRepository.Add(main);
            await _mainRepository.SaveChangesAsync();
        }

        public IQueryable<MAF_Main> QueryMain()
        {
            return _mainRepository.QueryMain();
        }
       

        public async Task UpdateAsync(MAF_Main main)
        {
            ValidateEntity(main);
            _mainRepository.Update(main);
           await _mainRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(MAF_Main main)
        {
            _mainRepository.Delete(main.MAF_No);
            await _mainRepository.SaveChangesAsync();
        }

        private void ValidateEntity(MAF_Main main)
        {
            if (main == null)
            {
                throw new ArgumentNullException(nameof(main), "MAF_Main entity cannot be null.");
            }

            var result = _validator.Validate(main);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new FluentValidation.ValidationException($"Entity validation failed: {string.Join(", ", errors)}");
            }
        }

    }
}