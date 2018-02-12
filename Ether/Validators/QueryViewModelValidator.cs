using Ether.Models;
using FluentValidation;

namespace Ether.Validators
{
    public class QueryViewModelValidator : AbstractValidator<QueryViewModel>
    {
        public QueryViewModelValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.QueryId).NotEmpty();
            RuleFor(p => p.Project).NotEmpty();
        }
    }
}
