using Ether.Interfaces;
using Ether.Models;
using Ether.Types.DTO;
using Ether.Types.Reporters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;
        private readonly PullRequestsReporter _pullRequestsReporter;

        public HomeController(IRepository repository, PullRequestsReporter pullRequestsReporter)
        {
            _repository = repository;
            _pullRequestsReporter = pullRequestsReporter;
        }

        public async Task<IActionResult> Index()
        {
            var allProfiles = await _repository.GetAllAsync<Profile>();
            var model = new ReportViewModel();
            model.Profiles = await GetAllProfiles();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Report(ReportViewModel model)
        {
            model.Profiles = await GetAllProfiles();
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            var report = await _pullRequestsReporter.ReportAsync(model.Profile, model.StartDate.Value, model.EndDate.Value);
            return RedirectToAction("View", "Reports", new { Id = report.Id });
        }

        private async Task<IEnumerable<SelectListItem>> GetAllProfiles()
        {
            var allProfiles = await _repository.GetAllAsync<Profile>();
            return allProfiles.Select(p => new SelectListItem() { Text = p.Name, Value = p.Id.ToString() });
        }
    }
}