using Ether.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;

namespace Ether.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IRepository _repository;

        public SettingsController(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> TeamMembers(TeamMembersViewModel model)
        {
            model = model ?? new TeamMembersViewModel();
            var members = (await _repository.GetAllAsync<TeamMember>())
                .OrderBy(m => m.TeamName);
            model.AllTeams = members.Select(m => m.TeamName)
                .Distinct();

            if (model.Teams == null)
            {
                model.Teams = Enumerable.Empty<string>();
                model.TeamMembers = members;
            }
            else
            {
                model.TeamMembers = members.Where(m => model.Teams.Contains(m.TeamName));
            }

            return View(model);
        }

        public async Task<IActionResult> EditMember(Guid? id)
        {
            if (!id.HasValue)
                return View(new TeamMemberViewModel());

            var member = await _repository.GetSingleAsync<TeamMember>(m => m.Id == id.Value);
            if (member == null)
                return View(new TeamMemberViewModel());

            var model = new TeamMemberViewModel
            {
                Id = member.Id,
                Email = member.Email,
                DisplayName = member.DisplayName,
                TeamName = member.TeamName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditMember(TeamMemberViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var member = new TeamMember
            {
                Id = model.Id.Value,
                Email = model.Email,
                DisplayName = model.DisplayName,
                TeamName = model.TeamName
            };

            await _repository.CreateOrUpdateAsync(member);
            return RedirectToAction(nameof(TeamMembers));
        }

        public async Task<IActionResult> DeleteMember(Guid id)
        {
             await _repository.DeleteAsync<TeamMember>(id);
            return RedirectToAction(nameof(TeamMembers));
        }

        public async Task<IActionResult> Projects()
        {
            var projects = await _repository.GetAllAsync<VSTSProject>();
            return View(projects.OrderBy(p => p.Name));
        }

        public async Task<IActionResult> EditProject(Guid? id)
        {
            if (!id.HasValue)
                return View(new VSTSProject { Id = Guid.NewGuid() });

            var project = await _repository.GetSingleAsync<VSTSProject>(p => p.Id == id.Value);
            if(project == null)
                return View(new VSTSProject { Id = Guid.NewGuid() });

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> EditProject(VSTSProject model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _repository.CreateOrUpdateAsync(model);
            return RedirectToAction(nameof(Projects));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            await _repository.DeleteAsync<VSTSProject>(id);
            return RedirectToAction(nameof(Projects));
        }

        public async Task<IActionResult> Repositories()
        {
            var repositories = (await _repository.GetAllAsync<VSTSRepository>())
                .OrderBy(r => r.Name);
            return View(repositories);
        }

        public async Task<IActionResult> EditRepository(Guid? id)
        {
            if (!id.HasValue)
                return View(new VSTSRepository { Id = Guid.NewGuid() });

            var repo = await _repository.GetSingleAsync<VSTSRepository>(r => r.Id == id.Value);
            if(repo == null)
                return View(new VSTSRepository { Id = Guid.NewGuid() });

            return View(repo);
        }

        [HttpPost]
        public async Task<IActionResult> EditRepository(VSTSRepository model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _repository.CreateOrUpdateAsync(model);
            return RedirectToAction(nameof(Repositories));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRepository(Guid id)
        {
            await _repository.DeleteAsync<VSTSRepository>(id);
            return RedirectToAction(nameof(Repositories));
        }
    }
}