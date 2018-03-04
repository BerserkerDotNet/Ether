using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO.Reports;
using Ether.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Pages.Reports
{
    [PageTitle("Report")]
    public class ViewModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly IEnumerable<IReporter> _reporters;

        public ViewModel(IRepository repository, IEnumerable<IReporter> reporters)
        {
            _repository = repository;
            _reporters = reporters;
        }

        public object Report { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id, bool print)
        {
            if (!id.HasValue)
                return RedirectToPage("Index");

            var reporterId = await _repository.GetFieldValueAsync<ReportResult, Guid>(r => r.Id == id.Value, r => r.ReporterId);
            var reporter = _reporters.SingleOrDefault(r => r.Id == reporterId);
            if (reporter == null)
            {
                TempData.WithError($"Report with {id} does not exist.");
                return RedirectToPage("Index");
            }

            var report = await _repository.GetSingleAsync(id.Value, reporter.ReportType);
            if (report == null)
            {
                TempData.WithError($"Report with {id} does not exist.");
                return RedirectToPage("Index");
            }

            Report = report;

            return Page();
        }
    }
}