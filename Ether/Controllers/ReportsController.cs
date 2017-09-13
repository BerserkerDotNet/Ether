using Ether.Extensions;
using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Ether.Core.Models.DTO.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IRepository _repository;

        public ReportsController(IRepository repository, IOptions<VSTSConfiguration> configuration)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var reports = (await _repository.GetAllAsync<ReportResult>())
                .OrderByDescending(r => r.DateTaken);
            return View(reports);
        }

        public async Task<IActionResult> View(Guid? id, [FromServices]IEnumerable<IReporter> reporters)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(Index));

            var reporterId = await _repository.GetFieldValue<ReportResult, Guid>(r => r.Id == id.Value, r => r.ReporterId);
            var reporter = reporters.SingleOrDefault(r => r.Id == reporterId);
            if (reporter == null)
            {
                TempData.WithError($"Report with {id} does not exist.");
                return RedirectToAction(nameof(Index));
            }

            var report = await _repository.GetSingleAsync(id.Value, reporter.ReportType);
            if (report == null)
            {
                TempData.WithError($"Report with {id} does not exist.");
                return RedirectToAction(nameof(Index));
            }

            return View(report);
        }
    }
}