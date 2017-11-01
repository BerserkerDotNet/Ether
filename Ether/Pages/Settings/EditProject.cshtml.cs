using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Models.DTO;
using Ether.Core.Interfaces;

namespace Ether.Pages.Settings
{
    public class EditProjectModel : PageModel
    {
        private readonly IRepository _repository;

        public EditProjectModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public VSTSProject Project { get; set; }

        public async Task OnGet(Guid? id)
        {
            if (!id.HasValue)
            {
                Project = new VSTSProject { Id = Guid.NewGuid() };
                return;
            }

            var project = await _repository.GetSingleAsync<VSTSProject>(p => p.Id == id.Value);
            if (project == null)
            {
                Project = new VSTSProject { Id = Guid.NewGuid() };
                return;
            }

            Project = project;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _repository.CreateOrUpdateAsync(Project);
            return RedirectToPage("Projects");
        }
    }
}