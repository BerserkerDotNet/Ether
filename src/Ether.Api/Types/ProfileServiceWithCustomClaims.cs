using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace Ether.Api.Types
{
    public class ProfileServiceWithCustomClaims : DefaultProfileService
    {
        private readonly IRepository _repository;

        public ProfileServiceWithCustomClaims(IRepository repository, ILogger<DefaultProfileService> logger)
            : base(logger)
        {
            _repository = repository;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.RequestedClaimTypes = new[] { ClaimTypes.Name, CustomClaims.DisplayName, ClaimTypes.Email, CustomClaims.Id };
            await base.GetProfileDataAsync(context);

            var idClaim = context.Subject.FindFirst("Id");
            if (idClaim == null || string.IsNullOrEmpty(idClaim.Value))
            {
                return;
            }

            var id = Guid.Parse(idClaim.Value);
            var user = await _repository.GetSingleAsync<User>(id);
            if (user == null)
            {
                user = new User
                {
                    Id = id,
                    DisplayName = context.Subject.FindFirst("name")?.Value,
                    Email = context.Subject.FindFirst(ClaimTypes.Email)?.Value
                };
                await _repository.CreateAsync(user);
            }
        }
    }
}
