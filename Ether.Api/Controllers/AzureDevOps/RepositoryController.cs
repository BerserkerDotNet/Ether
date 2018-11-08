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
    public class RepositoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RepositoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        public Task<IEnumerable<VstsRepositoryViewModel>> GetAll()
        {
            return _mediator.RequestCollection<GetAllRepositories, VstsRepositoryViewModel>(new GetAllRepositories());
        }

        [HttpPost]
        [Route(nameof(Save))]
        public Task Save(VstsRepositoryViewModel model)
        {
            return _mediator.Execute(new SaveRepository { Repository = model });
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public Task Delete(Guid id)
        {
            return _mediator.Execute(new DeleteRepository { Id = id });
        }
    }
}