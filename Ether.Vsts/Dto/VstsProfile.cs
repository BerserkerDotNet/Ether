using System;
using System.Collections.Generic;
using Ether.Contracts.Attributes;
using Ether.Contracts.Dto;
using VSTS.Net.Types;

namespace Ether.Vsts.Dto
{
    [DbName(nameof(Profile))]
    public class VstsProfile : Profile
    {
        public VstsProfile()
            : base(Constants.VstsType)
        {
        }

        public IEnumerable<Guid> Members { get; set; }

        public IEnumerable<Guid> Repositories { get; set; }
    }
}
