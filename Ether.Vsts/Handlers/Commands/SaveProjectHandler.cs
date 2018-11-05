using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Vsts.Commands;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Handlers.Commands
{
    public class SaveProjectHandler : ICommandHandler<SaveProject>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveProjectHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SaveProject input)
        {
            if (input == null || input.Project == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var dto = _mapper.Map<Project>(input.Project);
            await _repository.CreateOrUpdateAsync(dto);
        }
    }
}
