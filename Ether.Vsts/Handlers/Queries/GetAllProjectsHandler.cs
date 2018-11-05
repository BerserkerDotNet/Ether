using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Queries;

namespace Ether.Vsts.Handlers.Queries
{
    public class GetAllProjectsHandler : IQueryHandler<GetAllProjects, IEnumerable<VstsProjectViewModel>>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllProjectsHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VstsProjectViewModel>> Handle(GetAllProjects input)
        {
            var result = await _repository.GetAllAsync<Project>();
            return _mapper.Map<IEnumerable<VstsProjectViewModel>>(result);
        }
    }
}
