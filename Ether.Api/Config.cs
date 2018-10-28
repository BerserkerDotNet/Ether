using System.Collections.Generic;
using IdentityServer4.Models;

namespace Ether.Api
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "Ether API")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "EtherBlazorClient",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api" },
                    AllowedCorsOrigins = new[] { "http://localhost:57796" },
                    RedirectUris = { "http://localhost:57796" }
                }
            };
        }
    }
}
