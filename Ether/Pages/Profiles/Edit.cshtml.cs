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
    [PageTitle("Edit Profile")]
    public class EditModel : PageModel
    {
        private readonly IRepository _repository;

        public EditModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public ProfileViewModel Profile { get; set; }

        public async Task<IActionResult> OnGet(Guid? id)
        {
            Profile = new ProfileViewModel();
            Profile.Repositories = Enumerable.Empty<Guid>();
            Profile.Members = Enumerable.Empty<Guid>();
            Profile.Id = Guid.NewGuid();
            if (id.HasValue)
            {
                var profiles = await GetAllProfiles();
                Profile = profiles.SingleOrDefault(p => p.Id == id.Value);
            }

            if (Profile == null)
            {
                TempData.WithError($"Profile with id = '{id.Value}' does not exist.");
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var profile = new Profile
            {
                Id = Profile.Id,
                Name = Profile.Name,
                Repositories = Profile.Repositories,
                Members = Profile.Members
            };
            await _repository.CreateOrUpdateAsync(profile);

            TempData.WithSuccess($"Profile '{Profile.Name}' saved successfully!");
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