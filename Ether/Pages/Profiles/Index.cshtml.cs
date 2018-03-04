using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Extensions;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Pages.Profiles
{
    [PageTitle("Profiles")]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;

        public IndexModel(IRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public async Task OnGet()
        {
            Profiles = await GetAllProfiles();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var isSuccess = await _repository.DeleteAsync<Profile>(id);
            if (!isSuccess)
                TempData.WithError($"Profile with id = '{id}' does not exist.");

            return RedirectToPage("Index");
        }

        private async Task<IEnumerable<ProfileViewModel>> GetAllProfiles()
        {
            return (await _repository.GetAllAsync<Profile>()).Select(p => new ProfileViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Repositories = p.Repositories,
                Members = p.Members
            });
        }
    }
}