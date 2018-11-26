using System;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(Generate))]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Generate(string type, string dataSource, Guid profile, DateTime start, DateTime end)
        {
            var id = await _mediator.Execute<GeneratePullRequestsReport, Guid>(new GeneratePullRequestsReport { DataSourceType = dataSource, Profile = profile, Start = start, End = end });
            return Ok(id);
        }
    }
}