using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers.AzureDevOps
{
    [Route("api/vsts/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<IEnumerable<ProfileViewModel>> GetAll()
        {
            var profiles = await _mediator.RequestCollection<GetAllProfiles, ProfileViewModel>(new GetAllProfiles());
            return profiles;
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(ProfileViewModel model)
        {
            return _mediator.Execute(new SaveProfile { Profile = model });
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return _mediator.Execute(new DeleteProfile { Id = id });
        }
    }
}