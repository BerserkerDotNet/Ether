using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class TeamMemberViewModelValidator : AbstractValidator<TeamMemberViewModel>
    {
        public TeamMemberViewModelValidator()
        {
            RuleFor(m => m.Email).NotEmpty();
            RuleFor(m => m.DisplayName).NotEmpty();
        }
    }
}
