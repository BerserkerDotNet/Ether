using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(m => m.UserName).NotEmpty();
            RuleFor(m => m.Password).NotEmpty();
        }
    }
}
