using Ether.Extensions;
using Ether.Core.Interfaces;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Models;

namespace Ether.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;
        private readonly IEnumerable<IReporter> _repoters;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IRepository repository, IEnumerable<IReporter> repoters, ILogger<HomeController> logger)
        {
            _repository = repository;
            _repoters = repoters;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Loading main page");
            var model = new ReportViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Report(ReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            try
            {
                var reporter = _repoters.SingleOrDefault(r => r.Id == model.Report);
                if (reporter == null)
                {
                    TempData.WithError($"Selected report cannot be handled!");
                    return RedirectToAction(nameof(Index));
                }

                var report = await reporter.ReportAsync(new ReportQuery
                {
                    ProfileId = model.Profile,
                    StartDate = model.StartDate.Value,
                    EndDate = model.EndDate.Value
                });

                return RedirectToAction("View", "Reports", new { Id = report.Id });
            }
            catch(Exception ex)
            {
                TempData.WithError($"Error while generating report: '{ex.Message}'");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}