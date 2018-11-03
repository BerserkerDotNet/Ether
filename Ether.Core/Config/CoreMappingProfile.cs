using AutoMapper;
using Ether.Contracts.Dto;
using Ether.ViewModels;

namespace Ether.Core.Config
{
    public class CoreMappingProfile : Profile
    {
        public CoreMappingProfile()
        {
            CreateMap<Identity, IdentityViewModel>();
            CreateMap<IdentityViewModel, Identity>();
        }
    }
}
