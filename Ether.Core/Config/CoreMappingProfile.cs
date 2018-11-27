using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.ViewModels;

namespace Ether.Core.Config
{
    public class CoreMappingProfile : AutoMapper.Profile
    {
        public CoreMappingProfile()
        {
            CreateMap<Identity, IdentityViewModel>();
            CreateMap<IdentityViewModel, Identity>();
            CreateMap<PullRequestsReport, PullRequestReportViewModel>();
        }
    }
}
