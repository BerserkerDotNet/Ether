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
        public GetAllHandler(IRepository repository, IMapper mapper)
        {
            Repository = repository;
            Mapper = mapper;
        }

        public IRepository Repository { get; private set; }

        public IMapper Mapper { get; private set; }

        public async Task<IEnumerable<TModel>> Handle(TQuery query)
        {
            var result = await Repository.GetAllAsync<TData>();
            return await PostProcessData(Mapper.Map<IEnumerable<TModel>>(result));
        }

        protected virtual Task<IEnumerable<TModel>> PostProcessData(IEnumerable<TModel> data)
        {
            return Task.FromResult(data);
        }
    }
}
