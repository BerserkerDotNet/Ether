using System.Collections.Generic;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;

namespace Ether.Core.Types.Queries
{
    public abstract class GetAllPagedQuery<TModel> : IQuery<PageViewModel<TModel>>
    {
        public int Page { get; set; } = 1;

        public int ItemsPerPage { get; set; } = 10;
    }
}
