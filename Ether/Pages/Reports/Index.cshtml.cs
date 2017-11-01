using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Models.DTO.Reports;
using System.Threading.Tasks;
using Ether.Core.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System;
using Ether.Extensions;

namespace Ether.Pages.Reports
{
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