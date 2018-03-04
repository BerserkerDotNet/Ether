using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Extensions;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Ether.Pages.Queries
{
    [PageTitle("Query history")]
    public class HistoryModel : PageModel
    {
        private readonly IRepository _repository;

        public HistoryModel(IRepository repository)
        {
            _repository = repository;
        }

        public QueryViewModel Query { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (!id.HasValue)
            {
                TempData.WithError("No query selected");
                return RedirectToPage("Index");
            }

            var query = await _repository.GetSingleAsync<Query>(id.Value);
            if (query == null)
            {
                TempData.WithError("Query does not exists");
                return RedirectToPage("Index");
            }

            Query = new QueryViewModel
            {
                Id = query.Id,
                Name = query.Name,
                QueryId = query.QueryId
            };

            return Page();
        }
    }
}