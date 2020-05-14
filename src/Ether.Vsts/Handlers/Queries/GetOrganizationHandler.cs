using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetOrganizationHandler : IQueryHandler<GetOrganization, OrganizationViewModel>
    {
        private readonly IRepository _repository;

        public GetOrganizationHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrganizationViewModel> Handle(GetOrganization input)
        {
            var setting = await _repository.GetSingleAsync<Organization>(s => s.Type == Constants.VstsType);
            if (setting == null)
            {
                return null;
            }

            return new OrganizationViewModel
            {
                Id = setting.Id,
                Identity = setting.Identity,
                Name = setting.Name
            };
        }
    }
}
