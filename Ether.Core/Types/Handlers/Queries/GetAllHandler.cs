using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;

namespace Ether.Core.Types.Handlers.Queries
{
    public abstract class GetAllHandler<TData, TModel, TQuery> : IQueryHandler<TQuery, IEnumerable<TModel>>
        where TQuery : IQuery<IEnumerable<TModel>>
        where TData : BaseDto
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetAllHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TModel>> Handle(TQuery query)
        {
            var result = await _repository.GetAllAsync<TData>();
            return _mapper.Map<IEnumerable<TModel>>(result);
        }
    }
}
