using Ether.Models;
using FluentValidation;

namespace Ether.Validators
{
    public class TeamMemberViewModelValidator : AbstractValidator<TeamMemberViewModel>
    {
        public TeamMemberViewModelValidator()
        {
            RuleFor(m => m.Email).NotEmpty()
                .EmailAddress();
            RuleFor(m => m.TeamName).NotEmpty();
            RuleFor(m => m.DisplayName).NotEmpty();
        }
    }
}
