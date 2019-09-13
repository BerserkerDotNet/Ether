using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Core.Types.Commands;
using Ether.ViewModels;

namespace Ether.Core.Config
{
    public class CoreMappingProfile : AutoMapper.Profile
    {
        public CoreMappingProfile()
        {
            CreateMap<IdentityViewModel, Identity>();
            CreateMap<Identity, IdentityViewModel>()
                .ForMember(m => m.Token, a => a.Ignore());
            CreateMap<PullRequestsReport, PullRequestReportViewModel>();
            CreateMap<AggregatedWorkitemsETAReport, AggregatedWorkitemsETAReportViewModel>();
            CreateMap<WorkItemsReport, WorkItemsReportViewModel>();
            CreateMap<ReportJobState, JobLog>()
                .ForMember(l => l.Id, o => o.MapFrom(m => m.JobId));
            CreateMap<JobLog, JobLogViewModel>();
        }
    }
}
