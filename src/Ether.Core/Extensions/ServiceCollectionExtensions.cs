using Ether.Contracts.Dto.Reports;
using Ether.Core.Types;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ether.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddReporter<TCommand, TDto, TViewModel>(this IServiceCollection services, string uniqueName, string displayName)
            where TCommand : GenerateReportCommand
            where TDto : ReportResult
            where TViewModel : ReportViewModel
        {
            services.AddSingleton(new ReporterDescriptor(
                uniqueName,
                typeof(TCommand),
                typeof(TDto),
                typeof(TViewModel),
                displayName));
        }
    }
}
