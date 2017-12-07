using System;

namespace Ether.Core.Models.VSTS.Response
{
    public class IdentityResponse
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}
