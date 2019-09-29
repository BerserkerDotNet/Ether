using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Ether.Api.Attributes;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        public static readonly string WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;

        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;

        public LoginController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            TestUserStore users = null)
        {
            _users = new TestUserStore(new List<TestUser>());
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }

        [HttpGet]
        public async Task<IActionResult> Challenge(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "~/";
            }

            return await ProcessWindowsLoginAsync(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
            if (user == null)
            {
                user = AutoProvisionUser(provider, providerUserId, claims);
            }

            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();

            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));
            await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, localSignInProps, additionalLocalClaims.ToArray());

            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
            return Redirect(returnUrl);
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("Callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                await HttpContext.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                return base.Challenge(WindowsAuthenticationSchemeName);
            }
        }

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
        private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = _users.FindByExternalProvider(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private TestUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
            return user;
        }
    }
}
