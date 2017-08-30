using Ether.Models;
using FluentValidation;
using System.Linq;

namespace Ether.Types.Validators
{
    public class ProfileViewModelValidator : AbstractValidator<ProfileViewModel>
    {
        public ProfileViewModelValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Repositories)
                .Must(c => c != null && c.Any()).WithMessage("You have to have at least one repository.");
            RuleFor(p => p.Members)
                .Must(c => c != null && c.Any()).WithMessage("You have to have at least one team member.");
        }
    }
}
