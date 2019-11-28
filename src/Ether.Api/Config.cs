using System;
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
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = { "api", "openid" },
                    RequireClientSecret = false,
                    AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds
                },
                new Client
                {
                    ClientId = "EtherEmailGenerator",
                    ClientSecrets = new[] { new Secret("EtherEmailGeneratorSecret".Sha256()) }, // TODO: uhh...DB
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "api", "openid" },
                    RequireClientSecret = true,
                    AccessTokenLifetime = (int)TimeSpan.FromDays(1).TotalSeconds
                }
            };
        }
    }
}
