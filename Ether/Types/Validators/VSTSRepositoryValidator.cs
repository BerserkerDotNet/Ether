using Ether.Types.DTO;
using FluentValidation;

namespace Ether.Types.Validators
{
    public class VSTSRepositoryValidator : AbstractValidator<VSTSRepository>
    {
        public VSTSRepositoryValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Project).NotEmpty();
        }
    }
}
