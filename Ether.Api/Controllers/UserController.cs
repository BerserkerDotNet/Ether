using System;
using System.Security.Claims;
using Ether.Api.Types;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("GetUser")]
        public IActionResult Get()
        {
            var claims = User.Identity as ClaimsIdentity;
            var idClaim = claims.FindFirst(CustomClaims.Id)?.Value;

            var id = string.IsNullOrEmpty(idClaim) ? Guid.Empty : Guid.Parse(idClaim);
            var model = new UserViewModel
            {
                Id = id,
                DisplayName = claims.Name,
                Email = claims.FindFirst(ClaimTypes.Email)?.Value
            };
            return Ok(model);
        }

        [HttpGet]
        [Route(nameof(HasMenuAccess))]
        public IActionResult HasMenuAccess(string path, string category)
        {
            return Ok(true);
        }
    }
}