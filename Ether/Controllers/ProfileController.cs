using Ether.Extensions;
using Ether.Interfaces;
using Ether.Models;
using Ether.Types.DTO;
using Ether.Types.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly IRepository _repository;

        public ProfilesController(IRepository repository)
        {
            _repository = repository;
        }

        [IHave(typeof(VSTSRepository))]
        [IHave(typeof(TeamMember))]
        public async Task<IActionResult> Index()
        {
            var allProfiles = await GetAllProfiles();
            return View(allProfiles);
        }

        [IHave(typeof(VSTSRepository))]
        [IHave(typeof(TeamMember))]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var model = new ProfileViewModel();
            model.Repositories = Enumerable.Empty<Guid>();
            model.Members = Enumerable.Empty<Guid>();
            model.Id = Guid.NewGuid();
            if (id.HasValue)
            {
                var profiles = await GetAllProfiles();
                model = profiles.SingleOrDefault(p => p.Id == id.Value);
            }

            if (model == null)
            {
                TempData.WithError($"Profile with id = '{id.Value}' does not exist.");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [IHave(typeof(VSTSRepository))]
        [IHave(typeof(TeamMember))]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var profile = new Profile
            {
                Id = model.Id,
                Name = model.Name,
                Repositories = model.Repositories,
                Members = model.Members
            };
            await _repository.CreateOrUpdateAsync(profile);

            TempData.WithSuccess($"Profile '{model.Name}' saved successfully!");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isSuccess = await _repository.DeleteAsync<Profile>(id);
            if (!isSuccess)
                TempData.WithError($"Profile with id = '{id}' does not exist.");

            return RedirectToAction(nameof(Index));
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