using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Ether.Pages.Queries
{
    [PageTitle("Edit Query")]
    public class EditModel : PageModel
    {
        private readonly IRepository _repository;

        public EditModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public QueryViewModel Query { get; set; }

        public async Task OnGetAsync(Guid? id)
        {
            var queryModel = new QueryViewModel { Id = Guid.NewGuid() };
            if (id.HasValue)
            {
                var query = await _repository.GetSingleAsync<Query>(id.Value);
                if (query != null)
                {
                    queryModel.Id = query.Id;
                    queryModel.QueryId = query.QueryId;
                    queryModel.Name = query.Name;
                    queryModel.Project = query.Project;
                }
            }

            Query = queryModel;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var query = new Query
            {
                Id = Query.Id,
                QueryId = Query.QueryId.Value,
                Name = Query.Name,
                Project = Query.Project
            };

            await _repository.CreateOrUpdateAsync(query);
            return RedirectToPage("Index");
        }
    }
}