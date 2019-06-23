using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(m => m.UserName).NotEmpty();
            RuleFor(m => m.UserName).Must(n =>
            {
                if (string.IsNullOrEmpty(n))
                {
                    return true;
                }

                var domainAndName = n.Split('\\');
                if (domainAndName.Length != 2)
                {
                    return false;
                }

                return true;
            }).WithMessage("User name shouild be in the format of 'domain\\username'");
            RuleFor(m => m.Password).NotEmpty();
        }
    }
}
