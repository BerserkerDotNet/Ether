using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
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
            var model = await _mediator.Request<GetVstsDataSourceConfiguration, VstsDataSourceViewModel>(new GetVstsDataSourceConfiguration());
            return Ok(model);
        }

        [HttpPost]
        [Route("VstsDataSourceConfiguration")]
        public async Task<ActionResult> SaveVstsDataSourceConfiguration(VstsDataSourceViewModel model)
        {
            await _mediator.Execute(new SaveVstsDataSourceConfiguration { Configuration = model });
            return Ok();
        }
    }
}