using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Extensions;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Ether.Pages.Settings
{
    [PageTitle("Configuration")]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;

        public IndexModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public Ether.Core.Models.DTO.Settings Settings { get; set; }

        [BindProperty]
        public PersonalSettingsViewModel PersonalSettings { get; set; }

        public async Task OnGetAsync()
        {
            Settings = await _repository.GetSingleAsync<Ether.Core.Models.DTO.Settings>(_ => true);
            PersonalSettings = new PersonalSettingsViewModel();
            var result = await GetCurrentSettings();
            if (result != null)
            {
                PersonalSettings.TeamProfile = result.MyTeamProfile;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _repository.CreateOrUpdateAsync(Settings);
            TempData.WithSuccess("Settings were saved.");

            return RedirectToPage("/Settings/Index");
        }

        public async Task<IActionResult> OnPostPersonalAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await GetCurrentSettings();
            result = result ?? new PersonalSettings { Owner = User.Identity.Name };
            result.MyTeamProfile = PersonalSettings.TeamProfile;
            await _repository.CreateOrUpdateAsync(result);

            TempData.WithSuccess("Personal settings saved.");

            return RedirectToPage();
        }

        private async Task<PersonalSettings> GetCurrentSettings()
        {
            return await _repository.GetSingleAsync<PersonalSettings>(s => s.Owner == User.Identity.Name);
        }
    }
}