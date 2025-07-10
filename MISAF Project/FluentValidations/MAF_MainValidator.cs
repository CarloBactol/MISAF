using FluentValidation;
using MISAF_Project.EDMX;

namespace MISAF_Project.FluentValidations
{
    public class MAF_MainValidator : AbstractValidator<MAF_Main>
    {
        public MAF_MainValidator()
        {
            RuleFor(x => x.MAF_No)
                .NotEmpty().WithMessage("MAF_No is required")
                .Length(12).WithMessage("MAF_No must be exactly 12 characters long");

            RuleFor(x => x.Requestor_ID_No)
                .NotEmpty().WithMessage("Requestor_ID_No is required");

            RuleFor(x => x.Requestor_Name)
                .NotEmpty().WithMessage("Requestor_Name is required");

            RuleFor(x => x.Requestor_Div_Dep)
                .NotEmpty().WithMessage("Requestor_Div_Dep is required");

            RuleFor(x => x.Requestor_Workplace)
                .NotEmpty().WithMessage("Requestor_Workplace is required");

            RuleFor(x => x.PreApproved)
                .NotEmpty().WithMessage("PreApproved is required")
                .Must(x => x == "Y" || x == "N").WithMessage("PreApproved must be 'Y' or 'N'");

            RuleFor(x => x.DateTime_Requested)
                .NotEmpty().WithMessage("DateTime_Requested is required");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required");

            RuleFor(x => x.Status_DateTime)
                .NotEmpty().WithMessage("Status_DateTime is required");

            RuleFor(x => x.Encoded_By)
                .NotEmpty().WithMessage("Encoded_By is required");

            RuleFor(x => x.DateTime_Encoded)
                .NotEmpty().WithMessage("DateTime_Encoded is required");
        }
    }
}