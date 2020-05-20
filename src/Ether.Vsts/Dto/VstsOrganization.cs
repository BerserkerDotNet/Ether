using Ether.Contracts.Attributes;
using Ether.Contracts.Dto;

namespace Ether.Vsts.Dto
{
    [DbName(nameof(Organization))]
    public class VstsOrganization : Organization
    {
        public VstsOrganization()
            : base(Constants.VstsType)
        {
        }
    }
}
