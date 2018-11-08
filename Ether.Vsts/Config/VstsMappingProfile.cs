using AutoMapper;
using Ether.ViewModels;
using Ether.Vsts.Dto;

namespace Ether.Vsts.Config
{
    public class VstsMappingProfile : Profile
    {
        public VstsMappingProfile()
        {
            CreateMap<Project, VstsProjectViewModel>();
            CreateMap<VstsProjectViewModel, Project>();

            CreateMap<VstsProfile, VstsProfileViewModel>()
                .ForMember(p => p.Type, o => o.UseValue(Constants.VstsProfileType));
            CreateMap<VstsProfileViewModel, VstsProfile>()
                .ForMember(p => p.Type, o => o.UseValue(Constants.VstsProfileType));
        }
    }
}
