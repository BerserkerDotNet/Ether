using Newtonsoft.Json;
using System;

namespace Ether.Core.Models.VSTS.Response
{
    public class IdentityResponse
    {
        public Guid Id { get; set; }

        [JsonProperty("providerDisplayName")]
        public string DisplayName { get; set; }

        public bool IsActive { get; set; }
    }
}
