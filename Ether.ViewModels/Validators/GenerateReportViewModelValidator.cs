using System;
using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class GenerateReportViewModelValidator : AbstractValidator<GenerateReportViewModel>
    {
        public GenerateReportViewModelValidator()
        {
            RuleFor(r => r.Profile)
                .NotEmpty()
                .NotEqual(Guid.Empty);
            RuleFor(r => r.Start)
                .NotEmpty()
                .NotEqual(DateTime.MinValue)
                .LessThan(p => p.End);
            RuleFor(r => r.End)
                .NotEmpty()
                .NotEqual(DateTime.MinValue)
                .GreaterThan(r => r.Start)
                .LessThan(_ => DateTime.Now.AddDays(1)).WithMessage("End date shouldn't be more than 1 day in the future");
        }
    }
}
