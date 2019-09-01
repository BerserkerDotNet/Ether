using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Actions.Async
{
    public class ViewReport : IAsyncAction<ReportViewModel>
    {
        private readonly IUriHelper _uriHelper;

        public ViewReport(IUriHelper uriHelper)
        {
            _uriHelper = uriHelper;
        }

        public Task Execute(IDispatcher dispatcher, ReportViewModel report)
        {
            _uriHelper.NavigateTo($"/reports/view/{report.ReportType}/{report.Id}");
            return Task.CompletedTask;
        }
    }
}
