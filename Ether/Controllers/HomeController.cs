using Ether.Extensions;
using Ether.Interfaces;
using Ether.Models;
using Ether.Types.DTO;
using Ether.Types.DTO.Reports;
using Ether.Types.Filters;
using Ether.Types.Reporters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Ether.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;
        private readonly PullRequestsReporter _pullRequestsReporter;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IRepository repository, PullRequestsReporter pullRequestsReporter, ILogger<HomeController> logger)
        {
            _repository = repository;
            _pullRequestsReporter = pullRequestsReporter;
            _logger = logger;
        }

        [IHave(typeof(Profile))]
        public IActionResult Index()
        {
            _logger.LogInformation("Loading main page");
            var model = new ReportViewModel();
            return View(model);
        }

        [HttpPost]
        [IHave(typeof(Profile))]
        public async Task<IActionResult> Report(ReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            try
            {
                var report = await _pullRequestsReporter.ReportAsync(new ReportQuery
                {
                    ProfileId = model.Profile,
                    StartDate =  model.StartDate.Value,
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