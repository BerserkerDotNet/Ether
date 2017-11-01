using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Interfaces;
using Ether.Models;
using Ether.Core.Models.DTO;

namespace Ether.Pages.Settings
{
    public class EditMemberModel : PageModel
    {
        private readonly IRepository _repository;

        public EditMemberModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public TeamMemberViewModel Member { get; set; }

        public async Task OnGet(Guid? id)
        {
            if (!id.HasValue)
            {
                Member = new TeamMemberViewModel();
                return;
            }

            var member = await _repository.GetSingleAsync<TeamMember>(m => m.Id == id.Value);
            if (member == null)
            {
                Member = new TeamMemberViewModel();
                return;
            }

            Member =  new TeamMemberViewModel
            {
                Id = member.Id,
                Email = member.Email,
                DisplayName = member.DisplayName,
                TeamName = member.TeamName
            };
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var member = new TeamMember
            {
                Id = Member.Id.Value,
                Email = Member.Email,
                DisplayName = Member.DisplayName,
                TeamName = Member.TeamName
            };

            await _repository.CreateOrUpdateAsync(member);
            return RedirectToPage("TeamMembers");
        }
    }
}