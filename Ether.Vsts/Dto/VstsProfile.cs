using System;
using System.Collections.Generic;
using Ether.Contracts.Attributes;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    [DbName(nameof(Profile))]
    public class VstsProfile : Profile
    {
        public VstsProfile()
            : base(Constants.VstsProfileType)
        {
        }

        public IEnumerable<Guid> Members { get; set; }

        public IEnumerable<Guid> Repositories { get; set; }
    }
}
