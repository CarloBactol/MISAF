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
    public class AttachmentsService : IAttachmentsService
    {
        private readonly IAttachmentsRepository _attachmentRepository;
        private readonly IValidator<MAF_Attachment> _validator;

        public AttachmentsService(IAttachmentsRepository attachmentRepository, IValidator<MAF_Attachment> validator)
        {
            _attachmentRepository = attachmentRepository;
            _validator = validator;
        }
        public async Task AddAsync(MAF_Attachment attachment)
        {
            ValidateEntity(attachment);
            _attachmentRepository.Add(attachment);
          await  _attachmentRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(MAF_Attachment attachment)
        {
            _attachmentRepository.Delete(attachment.Record_ID);
            await _attachmentRepository.SaveChangesAsync();
        }

        public IQueryable<MAF_Attachment> QueryAttachment()
        {
            return _attachmentRepository.QueryAttachment();
        }

        public async Task UpdateAsync(MAF_Attachment attachment)
        {
            ValidateEntity(attachment);
            _attachmentRepository.Update(attachment);
            await _attachmentRepository.SaveChangesAsync();
        }

        private void ValidateEntity(MAF_Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment), "MAF_Attachment entity cannot be null.");
            }

            var result = _validator.Validate(attachment);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new FluentValidation.ValidationException($"Entity validation failed: {string.Join(", ", errors)}");
            }
        }
    }
}