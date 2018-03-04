using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Filters;

namespace Ether.Pages.Settings
{
    [PageTitle("Repositories")]
    public class RepositoriesModel : PageModel
    {
        private readonly IRepository _repository;

        public RepositoriesModel(IRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<VSTSRepository> Repositories { get; set; }

        public async Task OnGet()
        {
            Repositories = (await _repository.GetAllAsync<VSTSRepository>())
                .OrderBy(r => r.Name);
        }

        public async Task<IActionResult> OnPostDelete(Guid id)
        {
            await _repository.DeleteAsync<VSTSRepository>(id);
            return RedirectToPage("Repositories");
        }
    }
}