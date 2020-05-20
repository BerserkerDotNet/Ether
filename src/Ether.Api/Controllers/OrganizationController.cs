using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class OrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.RequestCollection<GetAllOrganizations, OrganizationViewModel>();

            return Ok(result);
        }

        [HttpGet]
        [Route(nameof(GetById))]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Request<GetOrganizationById, OrganizationViewModel>();

            return Ok(result);
        }

        [HttpPost]
        [Route(nameof(Save))]
        public async Task<IActionResult> Save(OrganizationViewModel model)
        {
            await _mediator.Execute(new SaveOrganization { Organization = model });
            return Ok();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Execute(new DeleteOrganization { Id = id });
            return Ok();
        }
    }
}
