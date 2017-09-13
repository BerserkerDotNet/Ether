using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ether.Core.Filters
{
    public class CurrentMenuIndicatorFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var viewResult = context.Result as ViewResult;
            if (viewResult == null || context.Controller == null)
                return;

            var typeName = context.Controller
                .GetType()
                .Name
                .Replace(nameof(Controller), "");

            viewResult.ViewData["CurrentMenu"] = typeName;
        }
    }
}
