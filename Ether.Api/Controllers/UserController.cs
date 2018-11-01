using Ether.Contracts.Interfaces.CQS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("Name")]
        public IActionResult GetName()
        {
            return new JsonResult(User.Identity.Name);
        }

        [HttpGet]
        [Route(nameof(HasMenuAccess))]
        public IActionResult HasMenuAccess(string path, string category)
        {
            return Ok(true);
        }
    }
}