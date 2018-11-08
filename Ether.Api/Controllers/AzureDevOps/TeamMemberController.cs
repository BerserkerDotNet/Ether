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
    public class TeamMemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeamMemberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public Task<IEnumerable<VstsTeamMemberViewModel>> GetAll()
        {
            return _mediator.RequestCollection<GetAllTeamMembers, VstsTeamMemberViewModel>(new GetAllTeamMembers());
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(VstsTeamMemberViewModel model)
        {
            return _mediator.Execute(new SaveTeamMember { TeamMember = model });
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return _mediator.Execute(new DeleteTeamMember { Id = id });
        }
    }
}