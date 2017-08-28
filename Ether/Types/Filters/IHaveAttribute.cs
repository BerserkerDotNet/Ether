using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ether.Interfaces;

namespace Ether.Types.Filters
{
    public class IHaveAttribute : TypeFilterAttribute
    {
        public IHaveAttribute(Type modelType) : base(typeof(IHaveFilter))
        {
            Arguments = new[] { modelType };
        }

        private class IHaveFilter : IActionFilter
        {
            private readonly IRepository _repository;
            private readonly Type _modelType;

            public IHaveFilter(IRepository repository, Type modelType)
            {
                _repository = repository;
                _modelType = modelType;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var controller = context.Controller as Controller;
                if (controller == null)
                    return;

                var allItemsPropertyName = Utils.GetNameForAllItems(_modelType);
                controller.ViewData[allItemsPropertyName] = _repository.GetAll(_modelType);
            }
        }

    }
}
