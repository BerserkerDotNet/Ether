using System;
using Ether.Contracts.Attributes;

namespace Ether.Vsts.Dto
{
    [DbName(nameof(Contracts.Dto.Organization))]
    public class Organization : Contracts.Dto.Organization
    {
        public Organization()
            : base(Constants.VstsType)
        {
        }
    }
}
