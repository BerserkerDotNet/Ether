using Ether.Types;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ether.Core.Filters
{
    public class CurrentMenuIndicatorFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var pageResult = context.Result as PageResult;
            if (pageResult == null || context.Controller == null)
                return;

            var currentMenu = EtherMenu.Menu.Find(context.Controller.GetType());
            pageResult.ViewData["CurrentMenu"] = currentMenu;
        }
    }
}
