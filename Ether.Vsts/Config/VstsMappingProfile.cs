using System.Linq;
using AutoMapper;
using Ether.ViewModels;
using Ether.Vsts.Dto;
using Ether.Vsts.Types;

namespace Ether.Vsts.Config
{
    public class VstsMappingProfile : Profile
    {
        public VstsMappingProfile()
        {
            CreateMap<Project, VstsProjectViewModel>()
                .ReverseMap();

            CreateMap<VstsProfile, ProfileViewModel>()
                .ForMember(p => p.Type, o => o.MapFrom(_ => Constants.VstsType))
                .ReverseMap();

            CreateMap<PullRequestViewModel, PullRequest>()
                .ForMember(p => p.Id, p => p.Ignore());

            CreateMap<TeamMemberViewModel, TeamMember>()
                .ForMember(r => r.RelatedWorkItems, p => p.Ignore());

            CreateMap<TeamMember, TeamMemberViewModel>()
                .ForMember(r => r.WorkItemsCount, p => p.MapFrom(s => s.RelatedWorkItems.Count()));

            CreateMap<WorkItemViewModel, WorkItem>()
                .ForMember(m => m.Id, m => m.Ignore())
                .ReverseMap();

            CreateMap<Repository, RepositoryInfo>()
                .ForMember(r => r.Project, o => o.Ignore())
                .ForMember(r => r.Members, o => o.Ignore());

            CreateMap<Project, ProjectInfo>()
                .ForMember(r => r.Identity, o => o.Ignore());
        }
    }
}
