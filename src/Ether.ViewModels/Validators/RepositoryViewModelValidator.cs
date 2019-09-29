using FluentValidation;

namespace Ether.ViewModels.Validators
{
    public class RepositoryViewModelValidator : AbstractValidator<VstsRepositoryViewModel>
    {
        public RepositoryViewModelValidator()
        {
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Project).NotEmpty();
        }
    }
}
