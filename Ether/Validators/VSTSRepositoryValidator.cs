using Ether.Core.Models.DTO;
using FluentValidation;

namespace Ether.Validators
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
