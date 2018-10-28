using System.Linq;
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
        [HttpGet]
        [Route("Name")]
        public IActionResult GetName()
        {
            return new JsonResult(User.Identity.Name);
        }
    }
}