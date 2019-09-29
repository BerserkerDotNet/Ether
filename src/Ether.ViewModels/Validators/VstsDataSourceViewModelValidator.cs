using Ether.ViewModels;
using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class VstsDataSourceViewModelValidator : AbstractValidator<VstsDataSourceViewModel>
    {
        public VstsDataSourceViewModelValidator()
        {
            RuleFor(d => d.InstanceName).NotEmpty();
        }
    }
}
