using System;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;

namespace Ether.Core.Types.Handlers.Commands
{
    public class SaveIdentityHandler : ICommandHandler<SaveIdentity>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public SaveIdentityHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Handle(SaveIdentity input)
        {
            if (input == null || input.Identity == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var dto = _mapper.Map<Identity>(input.Identity);

            await _repository.CreateOrUpdateAsync(dto);
        }
    }
}
