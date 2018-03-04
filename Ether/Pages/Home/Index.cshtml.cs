using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Extensions;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Pages.Home
{
    [PageTitle("Home")]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly IEnumerable<IReporter> _repoters;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRepository repository, IEnumerable<IReporter> repoters, ILogger<IndexModel> logger)
        {
            _repository = repository;
            _repoters = repoters;
            _logger = logger;
        }

        [BindProperty]
        public ReportViewModel ReportRequest { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostReportAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var reporter = _repoters.SingleOrDefault(r => r.Id == ReportRequest.Report);
                if (reporter == null)
                {
                    TempData.WithError($"Selected report cannot be handled!");
                    return RedirectToPage("Index");
                }

                var report = await reporter.ReportAsync(new ReportQuery
                {
                    ProfileId = ReportRequest.Profile,
                    StartDate = ReportRequest.StartDate.Value.ToUniversalTime(),
                    EndDate = ReportRequest.EndDate.Value.ToUniversalTime()
                });

                return RedirectToPage("/Reports/View", new { Id = report.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report.");
                TempData.WithError($"Error while generating report: '{ex.Message}'");
                return RedirectToAction("Index");
            }
        }
    }
}