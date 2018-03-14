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
    [PageTitle("Personal")]
    public class PersonalModel : PageModel
    {
        private readonly IRepository _repository;

        public PersonalModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public PersonalSettingsViewModel Settings { get; set; }

        public async Task OnGet()
        {
            Settings = new PersonalSettingsViewModel();
            var result = await GetCurrentSettings();
            if (result != null)
            {
                Settings.TeamProfile = result.MyTeamProfile;
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await GetCurrentSettings();
            result = result ?? new PersonalSettings { Owner = User.Identity.Name };
            result.MyTeamProfile = Settings.TeamProfile;
            await _repository.CreateOrUpdateAsync(result);

            TempData.WithSuccess("Settings saved.");

            return RedirectToPage();
        }

        private async Task<PersonalSettings> GetCurrentSettings()
        {
            return await _repository.GetSingleAsync<PersonalSettings>(s => s.Owner == User.Identity.Name);
        }
    }
}