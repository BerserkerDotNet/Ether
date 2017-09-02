using Ether.Extensions;
using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.DTO.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
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

        public async Task<IActionResult> View(Guid? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(Index));

            var report = await _repository.GetSingleAsync<ReportResult>(r => r.Id == id.Value, r => Type.GetType(r.ReportType));
            if (report == null)
            {
                TempData.WithError($"Report with {id} does not exist.");
                return RedirectToAction(nameof(Index));
            }

            return View(report);
        }
    }
}