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
            CreateMap<Project, VstsProjectViewModel>();
            CreateMap<VstsProjectViewModel, Project>();

            CreateMap<VstsProfile, ProfileViewModel>()
                .ForMember(p => p.Type, o => o.MapFrom(_ => Constants.VstsType));
            CreateMap<ProfileViewModel, VstsProfile>()
                .ForMember(p => p.Type, o => o.MapFrom(_ => Constants.VstsType));

            CreateMap<Repository, RepositoryInfo>()
                .ForMember(r => r.Project, o => o.Ignore())
                .ForMember(r => r.Members, o => o.Ignore());

            CreateMap<Project, ProjectInfo>()
                .ForMember(r => r.Identity, o => o.Ignore());

            CreateMap<PullRequestViewModel, PullRequest>()
                .ForMember(p => p.Id, p => p.Ignore());

            CreateMap<TeamMemberViewModel, TeamMember>()
                .ForMember(r => r.RelatedWorkItems, p => p.Ignore());
        }
    }
}
