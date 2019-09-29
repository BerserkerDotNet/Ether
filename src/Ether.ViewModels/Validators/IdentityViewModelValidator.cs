using Ether.ViewModels;
using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class IdentityViewModelValidator : AbstractValidator<IdentityViewModel>
    {
        public IdentityViewModelValidator()
        {
            RuleFor(i => i.Name).NotEmpty();
            RuleFor(i => i.Token).NotEmpty();
        }
    }
}
