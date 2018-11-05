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
        }
    }
}
