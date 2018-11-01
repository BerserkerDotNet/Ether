using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("VstsDataSourceConfiguration")]
        public async Task<IActionResult> GetVstsDataSourceConfiguration()
        {
            var model = await _mediator.Request<GetVstsDataSourceConfigurationQuery, VstsDataSourceViewModel>(new GetVstsDataSourceConfigurationQuery());
            return new JsonResult(model);
        }
    }
}