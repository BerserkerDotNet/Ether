using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Pages.Settings
{
    [PageTitle("Projects")]
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