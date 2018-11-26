using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Exceptions;
using Ether.Vsts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers.AzureDevOps
{
    [Route("api/vsts/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class TeamMemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamMemberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public Task<IEnumerable<TeamMemberViewModel>> GetAll()
        {
            return _mediator.RequestCollection<GetAllTeamMembers, TeamMemberViewModel>(new GetAllTeamMembers());
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(TeamMemberViewModel model)
        {
            try
            {
                return _mediator.Execute(new SaveTeamMember { TeamMember = model });
            }
            catch (IdentityNotFoundException ex)
            {
                return Task.FromResult(BadRequest(ex.Message));
            }
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return _mediator.Execute(new DeleteTeamMember { Id = id });
        }
    }
}