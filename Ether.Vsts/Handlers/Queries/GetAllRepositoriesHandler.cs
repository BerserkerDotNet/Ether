using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetAllRepositoriesHandler : GetAllHandler<Repository, VstsRepositoryViewModel, GetAllRepositories>
    {
        public GetAllRepositoriesHandler(IRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }
}
