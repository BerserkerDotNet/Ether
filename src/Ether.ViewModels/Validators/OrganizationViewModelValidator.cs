using Ether.ViewModels;
using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class OrganizationViewModelValidator : AbstractValidator<OrganizationViewModel>
    {
        public OrganizationViewModelValidator()
        {
            RuleFor(d => d.Name).NotEmpty();
        }
    }
}
