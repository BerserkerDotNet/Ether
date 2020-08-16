using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using Ether.Contracts.Types.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Options;

namespace Ether.Api.Types
{
    public class ADPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IOptions<ADConfiguration> _adConfig;

        public ADPasswordValidator(IOptions<ADConfiguration> adConfig)
        {
            _adConfig = adConfig;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var type = (ContextType)Enum.Parse(typeof(ContextType), _adConfig.Value.Type);
            var domainAndName = context.UserName.Split('\\');
            if (domainAndName.Length != 2)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User name should have be in the format of 'domain\\username'");
                return Task.CompletedTask;
            }

            var domain = domainAndName[0];
            var userName = domainAndName[1];
            if (type == ContextType.Machine)
            {
                domain = Environment.MachineName;
            }

            using (var principal = new PrincipalContext(type, domain))
            {
                var isValid = principal.ValidateCredentials(userName, context.Password, ContextOptions.Negotiate);
                if (isValid)
                {
                    var userNameToSearch = type == ContextType.Machine ? Environment.UserName : context.UserName;
                    var user = UserPrincipal.FindByIdentity(principal, userNameToSearch);
                    var claims = GetAdditionalClaims(user);
                    context.Result = new GrantValidationResult(subject: user.Name, authenticationMethod: "ADS", claims: claims);
                }
                else
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User name or password is incorrect");
                }

                return Task.CompletedTask;
            }
        }

        private IEnumerable<Claim> GetAdditionalClaims(UserPrincipal user)
        {
            yield return new Claim(CustomClaims.DisplayName, user.DisplayName);

            if (user.Guid.HasValue)
            {
                yield return new Claim(CustomClaims.Id, user.Guid.Value.ToString());
            }

            if (!string.IsNullOrEmpty(user.EmailAddress))
            {
                yield return new Claim(ClaimTypes.Email, user.EmailAddress);
            }
        }
    }
}
