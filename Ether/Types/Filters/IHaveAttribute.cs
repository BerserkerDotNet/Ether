using Ether.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Ether.Types.Filters
{
    public class IHaveAttribute : TypeFilterAttribute
    {
        public IHaveAttribute(Type modelType) : base(typeof(IHaveFilter))
        {
            Arguments = new[] { modelType };
        }

        private class IHaveFilter : IAsyncActionFilter
        {
            private readonly IRepository _repository;
            private readonly Type _modelType;

            public IHaveFilter(IRepository repository, Type modelType)
            {
                _repository = repository;
                _modelType = modelType;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await AddItems(context);
                await next();
            }

            private async Task AddItems(ActionExecutingContext context)
            {
                var controller = context.Controller as Controller;
                if (controller == null)
                    return;

                var allItemsPropertyName = Utils.GetNameForAllItems(_modelType);
                controller.ViewData[allItemsPropertyName] = await _repository.GetAllByTypeAsync(_modelType);
            }
        }
    }
}
