using Ether.Models;
using FluentValidation;
using System;

namespace Ether.Types.Validators
{
    public class ReportViewModelValidator: AbstractValidator<ReportViewModel>
    {
        public ReportViewModelValidator()
        {
            RuleFor(r => r.Profile).NotNull();
            RuleFor(r => r.Report).NotNull();
            RuleFor(r => r.StartDate)
                .NotNull()
                .LessThan(p => p.EndDate)
                .GreaterThan(p => DateTime.Now.AddMonths(-6)).WithMessage("Date shouldn't be more than 6 month in the past");
            RuleFor(r => r.EndDate)
                .NotNull()
                .GreaterThan(p => p.StartDate)
                .LessThan(p => DateTime.Now.AddDays(1)).WithMessage("Date shouldn't be more than 1 day in the future"); ;
        }
    }
}
