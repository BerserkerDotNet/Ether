using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class OrganizationViewModelValidator : AbstractValidator<OrganizationViewModel>
    {
        public OrganizationViewModelValidator()
        {
            RuleFor(o => o.Name).NotEmpty();
            RuleFor(o => o.Type).NotEmpty();
            RuleFor(o => o.Identity).NotEmpty();
        }
    }
}
