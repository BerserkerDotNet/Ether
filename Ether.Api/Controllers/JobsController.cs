using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(Logs) + "/GetAll")]
        public async Task<IActionResult> Logs()
        {
            var logs = await _mediator.RequestCollection<GetAllJobLogs, JobLogViewModel>();
            return Ok(logs);
        }

        [HttpPost]
        [Route("vsts/" + nameof(RunWorkitemsJob))]
        public async Task<IActionResult> RunWorkitemsJob(RunWorkitemsJob command)
        {
            await _mediator.Execute(command);
            return Ok();
        }
    }
}