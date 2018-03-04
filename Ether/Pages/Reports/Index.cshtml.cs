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
    [PageTitle("Reports")]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;

        public IndexModel(IRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<ReportResult> Reports { get; set; }

        public async Task OnGetAsync()
        {
            Reports = (await _repository.GetAllAsync<ReportResult>())
                 .OrderByDescending(r => r.DateTaken);
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var isSuccess = await _repository.DeleteAsync<ReportResult>(id);
            if (!isSuccess)
                TempData.WithError($"Report with id = '{id}' does not exist.");

            return RedirectToPage("Index");
        }
    }
}