using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Models.DTO;
using Ether.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Ether.Pages.Settings
{
    public class ProjectsModel : PageModel
    {
        private readonly IRepository _repository;

        public ProjectsModel(IRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<VSTSProject> Projects { get; set; }

        public async Task OnGet()
        {
            var projects = await _repository.GetAllAsync<VSTSProject>();
            Projects = projects.OrderBy(p => p.Name);
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await _repository.DeleteAsync<VSTSProject>(id);
            return RedirectToPage("Projects");
        }
    }
}