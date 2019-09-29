using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class ProfileViewModelValidator : AbstractValidator<ProfileViewModel>
    {
        public ProfileViewModelValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Type).Equal("Vsts");
            RuleFor(p => p.Members).NotEmpty();
            RuleFor(p => p.Repositories).NotEmpty();
        }
    }
}
