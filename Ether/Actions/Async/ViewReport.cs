using System.Threading.Tasks;
using Ether.Redux.Interfaces;
using Ether.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ether.Actions.Async
{
    public class ViewReport : IAsyncAction<ReportViewModel>
    {
        private readonly NavigationManager _navigation;

        public ViewReport(NavigationManager uriHelper)
        {
            _navigation = uriHelper;
        }

        public Task Execute(IDispatcher dispatcher, ReportViewModel report)
        {
            _navigation.NavigateTo($"/reports/view/{report.ReportType}/{report.Id}");
            return Task.CompletedTask;
        }
    }
}
