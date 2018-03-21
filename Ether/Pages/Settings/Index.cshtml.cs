using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Extensions;
using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ether.Pages.Settings
{
    [PageTitle("Configuration")]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRepository repository, ILogger<IndexModel> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [BindProperty]
        public SettingsViewModel Settings { get; set; }

        public async Task OnGetAsync()
        {
            Settings = new SettingsViewModel();
            var generalSettings = await _repository.GetSingleAsync<Ether.Core.Models.DTO.Settings>(_ => true);
            if (generalSettings != null)
            {
                Settings.DisableWorkitemsJob = generalSettings.WorkItemsSettings?.DisableWorkitemsJob ?? false;
                Settings.KeepLastWorkItems = generalSettings.WorkItemsSettings?.KeepLast;

                Settings.DisablePullRequestsJob = generalSettings.PullRequestsSettings?.DisablePullRequestsJob ?? false;
                Settings.KeepLastPullRequests = generalSettings.PullRequestsSettings?.KeepLast;

                Settings.KeepLastReports = generalSettings.ReportsSettings?.KeepLast;

            }
            var result = await GetUserSettings();
            if (result != null)
            {
                Settings.TeamProfile = result.MyTeamProfile;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await SaveGeneralSettings();
                await SavePersonalSettings();

                TempData.WithSuccess("Settings were saved.");
                _logger.LogWarning("Settings were changed. New settings are '{CurrentSettings}'", Settings);
            }
            catch (System.Exception ex)
            {
                TempData.WithError($"Error saving settings: {ex.Message}");
                _logger.LogError(ex, "Error saving settings");
            }
            return RedirectToPage("/Settings/Index");
        }

        private async Task SaveGeneralSettings()
        {
            var generalSettings = new Ether.Core.Models.DTO.Settings();
            generalSettings.WorkItemsSettings = new Core.Models.DTO.Settings.WorkItems
            {
                DisableWorkitemsJob = Settings.DisableWorkitemsJob,
                KeepLast = Settings.KeepLastWorkItems
            };
            generalSettings.PullRequestsSettings = new Core.Models.DTO.Settings.PullRequests
            {
                DisablePullRequestsJob = Settings.DisablePullRequestsJob,
                KeepLast = Settings.KeepLastPullRequests
            };
            generalSettings.ReportsSettings = new Core.Models.DTO.Settings.Reports
            {
                KeepLast = Settings.KeepLastReports
            };

            await _repository.CreateOrUpdateAsync(generalSettings);
        }

        public async Task SavePersonalSettings()
        {
            var result = await GetUserSettings();
            result = result ?? new PersonalSettings { Owner = User.Identity.Name };
            result.MyTeamProfile = Settings.TeamProfile;
            await _repository.CreateOrUpdateAsync(result);
        }

        private async Task<PersonalSettings> GetUserSettings()
        {
            return await _repository.GetSingleAsync<PersonalSettings>(s => s.Owner == User.Identity.Name);
        }
    }
}