using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Filters;

namespace Ether.Pages.Settings
{
    [PageTitle("Edit Repository")]
    public class EditRepositoryModel : PageModel
    {
        private readonly IRepository _repository;

        public EditRepositoryModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public VSTSRepository Repository { get; set; }

        public async Task OnGet(Guid? id)
        {
            if (!id.HasValue)
            {
                Repository = new VSTSRepository { Id = Guid.NewGuid() };
                return;
            }

            var repository = await _repository.GetSingleAsync<VSTSRepository>(r => r.Id == id.Value);
            if (repository == null)
            {
                Repository = new VSTSRepository { Id = Guid.NewGuid() };
                return;
            }

            Repository = repository;
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            await _repository.CreateOrUpdateAsync(Repository);
            return RedirectToPage("Repositories");
        }
    }
}