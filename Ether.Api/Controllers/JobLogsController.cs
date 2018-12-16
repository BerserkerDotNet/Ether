using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JobLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public async Task<ActionResult> GetAll()
        {
            var logs = await _mediator.RequestCollection<GetAllJobLogs, JobLogViewModel>();
            return Ok(logs);
        }
    }
}