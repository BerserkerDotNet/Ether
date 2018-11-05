using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Contracts.Interfaces.CQS;
using Ether.ViewModels;
using Ether.Vsts.Commands;
using Ether.Vsts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ether.Api.Controllers.AzureDevOps
{
    [Route("api/vsts/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public Task<IEnumerable<VstsProjectViewModel>> GetAll()
        {
            return _mediator.RequestCollection<GetAllProjects, VstsProjectViewModel>(new GetAllProjects());
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(VstsProjectViewModel model)
        {
            return _mediator.Execute(new SaveProject { Project = model });
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return Task.CompletedTask;
        }
    }
}