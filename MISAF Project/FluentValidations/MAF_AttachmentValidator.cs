using FluentValidation;
using MISAF_Project.EDMX;


namespace MISAF_Project.FluentValidations
{
    public class MAF_AttachmentValidator : AbstractValidator<MAF_Attachment>
    {
        public MAF_AttachmentValidator()
        {
            RuleFor(x => x.MAF_No)
                .NotEmpty().WithMessage("MAF_No is required")
                .Length(12).WithMessage("MAF_No must be exactly 12 characters long");

            RuleFor(x => x.Filename)
                .NotEmpty().WithMessage("Filename is required");
        }
    }
}