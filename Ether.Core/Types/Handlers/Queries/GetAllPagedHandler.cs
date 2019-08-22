using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;

namespace Ether.Core.Types.Handlers.Queries
{
    public abstract class GetAllPagedHandler<TData, TModel, TQuery> : IQueryHandler<TQuery, PageViewModel<TModel>>
        where TQuery : GetAllPagedQuery<TModel>
        where TData : BaseDto
    {
        public GetAllPagedHandler(IRepository repository, IMapper mapper)
        {
            Repository = repository;
            Mapper = mapper;
        }

        public IRepository Repository { get; private set; }

        public IMapper Mapper { get; private set; }

        public async Task<PageViewModel<TModel>> Handle(TQuery query)
        {
            var result = await Repository.GetAllPagedAsync<TData>(query.Page, query.ItemsPerPage);
            var data = await PostProcessData(Mapper.Map<IEnumerable<TModel>>(result));

            var count = await Repository.CountAsync<TData>();
            return new PageViewModel<TModel>
            {
                Items = data,
                CurrentPage = query.Page,
                TotalPages = (int)Math.Ceiling(count / (double)query.ItemsPerPage)
            };
        }

        protected virtual Task<IEnumerable<TModel>> PostProcessData(IEnumerable<TModel> data)
        {
            return Task.FromResult(data);
        }
    }
}
