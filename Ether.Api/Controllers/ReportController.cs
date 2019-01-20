using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Extensions;
using Ether.Core.Types;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Queries;
using Ether.ViewModels;
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
        private readonly IEnumerable<ReporterDescriptor> _reporters;
        private readonly IMapper _mapper;

        public ReportController(IMediator mediator, IEnumerable<ReporterDescriptor> reporters, IMapper mapper)
        {
            _mediator = mediator;
            _reporters = reporters;
            _mapper = mapper;
        }

        [HttpPost]
        [Route(nameof(Generate))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Generate(GenerateReportViewModel requestModel)
        {
            var id = Guid.Empty;
            var reporter = _reporters.FirstOrDefault(r => string.Equals(r.UniqueName, requestModel.ReportType, StringComparison.OrdinalIgnoreCase));
            if (reporter == null)
            {
                return BadRequest($"Reporter of type {requestModel.ReportType} is not supported.");
            }

            id = await GenerateReport(reporter.CommandType, requestModel);
            return Ok(id);
        }

        [HttpGet]
        [Route(nameof(Types))]
        [ProducesResponseType(200)]
        public IActionResult Types()
        {
            var model = _mapper.MapCollection<ReporterDescriptorViewModel>(_reporters);
            return Ok(model);
        }

        [HttpGet]
        [Route(nameof(GetAll))]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _mediator.RequestCollection<GetAllReports, ReportViewModel>();
            return Ok(reports);
        }

        [HttpGet]
        [Route(nameof(GetById))]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var reports = await _mediator.Request<GetReportById, ReportViewModel>(new GetReportById(id));
            return Ok(reports);
        }

        private Task<Guid> GenerateReport(Type generateCommandType, GenerateReportViewModel requestModel)
        {
            var request = _mapper.Map(requestModel, typeof(GenerateReportViewModel), generateCommandType) as GenerateReportCommand;
            return _mediator.Execute<Guid>(request);
        }
    }
}