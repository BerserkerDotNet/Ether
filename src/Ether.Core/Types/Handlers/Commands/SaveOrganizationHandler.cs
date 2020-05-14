using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class SaveOrganizationHandler : ICommandHandler<SaveOrganization>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveOrganizationHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SaveOrganization input)
        {
            if (input == null || input.Organization == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var dto = _mapper.Map<Organization>(input.Organization);

            await _repository.CreateOrUpdateAsync(dto);
        }
    }
}
