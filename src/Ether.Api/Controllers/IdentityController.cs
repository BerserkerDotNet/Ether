using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IdentityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.RequestCollection<GetAllIdentities, IdentityViewModel>();
            return Ok(result);
        }

        [HttpGet]
        [Route(nameof(GetById))]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Request<GetIdentityById, IdentityViewModel>();
            return Ok(result);
        }

        [HttpPost]
        [Route(nameof(Save))]
        public async Task<IActionResult> Save(IdentityViewModel model)
        {
            await _mediator.Execute(new SaveIdentity { Identity = model });
            return Ok();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Execute(new DeleteIdentity { Id = id });
            return Ok();
        }
    }
}
