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

            CreateMap<VstsProfile, VstsProfileViewModel>()
                .ForMember(p => p.Type, o => o.UseValue(Constants.VstsType));
            CreateMap<VstsProfileViewModel, VstsProfile>()
                .ForMember(p => p.Type, o => o.UseValue(Constants.VstsType));

            CreateMap<Repository, RepositoryInfo>()
                .ForMember(r => r.Project, o => o.Ignore())
                .ForMember(r => r.Members, o => o.Ignore());

            CreateMap<Project, ProjectInfo>()
                .ForMember(r => r.Identity, o => o.Ignore());

            CreateMap<VstsPullRequestViewModel, PullRequest>()
                .ForMember(p => p.Id, p => p.Ignore());
        }
    }
}
