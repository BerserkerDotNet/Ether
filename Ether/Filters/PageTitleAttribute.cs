using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Ether.Core.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PageTitleAttribute : ResultFilterAttribute
    {
        private readonly string _title;

        public PageTitleAttribute(string title)
        {
            _title = title;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var pageResult = context.Result as PageResult;
            if (pageResult == null || context.Controller == null)
                return;

            pageResult.ViewData["Title"] = _title;
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
