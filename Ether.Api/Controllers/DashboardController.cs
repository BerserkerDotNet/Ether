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
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(ActiveWorkitems))]
        public async Task<IActionResult> ActiveWorkitems(Guid profileId)
        {
            var data = await _mediator.Request<GetActiveWorkitemsForProfile, ActiveWorkitemsViewModel>(new GetActiveWorkitemsForProfile { ProfileId = profileId });
            return Ok(data);
        }

        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<IActionResult> GetById(Guid id)
        {
            var settings = await _mediator.Request<GetDashboardSettings, DashboardSettingsViewModel>(new GetDashboardSettings(id));
            return Ok(settings);
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(DashboardSettingsViewModel model)
        {
            return _mediator.Execute(new SaveDashboardSettings { Settings = model });
        }

        /*
        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return _mediator.Execute(new DeleteDashboardSettings { Id = id });
        }*/
    }
}