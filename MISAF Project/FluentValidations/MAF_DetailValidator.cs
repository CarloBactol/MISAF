using FluentValidation;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.FluentValidations
{
    public class MAF_DetailValidator : AbstractValidator<MAF_Detail>
    {
        public MAF_DetailValidator()
        {
            RuleFor(x => x.MAF_No)
               .NotEmpty().WithMessage("MAF_No is required")
               .Length(12).WithMessage("MAF_No must be exactly 12 characters long");

            RuleFor(x => x.Category_ID)
                .NotEmpty().WithMessage("Category_ID is required");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

            RuleFor(x => x.Request)
                .NotEmpty().WithMessage("Request is required");

            RuleFor(x => x.Reason_ID)
              .NotEmpty().WithMessage("Reason_ID is required");

            RuleFor(x => x.Reason)
              .NotEmpty().WithMessage("Reason is required");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(30).WithMessage("Status cannot exceed 30 characters");
           
        }
    }
}