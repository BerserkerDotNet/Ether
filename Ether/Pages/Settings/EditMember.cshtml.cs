using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ether.Core.Interfaces;
using Ether.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Data;
using Ether.Core.Configuration;
using Microsoft.Extensions.Options;
using Ether.Core.Models.VSTS.Response;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Ether.Pages.Settings
{
    public class EditMemberModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly IVSTSClient _client;
        private readonly IOptions<VSTSConfiguration> _config;
        private readonly ILogger<EditMemberModel> _logger;

        public EditMemberModel(IRepository repository, IVSTSClient client, IOptions<VSTSConfiguration> config, ILogger<EditMemberModel> logger)
        {
            _repository = repository;
            _client = client;
            _config = config;
            _logger = logger;
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

            var identityUrl = VSTSApiUrl.Create(_config.Value.InstanceName, "vssps")
                .ForIdentities()
                .WithQueryParameter("searchFilter", "General")
                .WithQueryParameter("filterValue", Member.Email)
                .Build("4.0");

            var identities = await _client.ExecuteGet<ValueBasedResponse<IdentityResponse>>(identityUrl);
            var users = identities.Value
                .Where(u => u.IsActive)
                .ToList();

            if (users.Count == 1)
            {
                var member = new TeamMember
                {
                    Id = users[0].Id,
                    Email = Member.Email,
                    DisplayName = Member.DisplayName,
                    TeamName = Member.TeamName
                };

                await _repository.CreateOrUpdateAsync(member);
                return RedirectToPage("TeamMembers");
            }

            if (users.Count == 0)
            {
                var message = $"No active users found with email '{Member.Email}'; Count including inactive: {identities.Count}";
                _logger.LogWarning(message);
                ModelState.AddModelError($"{nameof(Member)}.{nameof(Member.Email)}", $"No active users found with email '{Member.Email}'");
               
            }
            else if (users.Count > 1)
            {
                var usersList = string.Join("; ", users.Select(u => $"{u.DisplayName} [{u.Id}]"));
                var message = $"More than one active user found with email '{Member.Email}'. List: {usersList}";
                _logger.LogWarning(message);
                ModelState.AddModelError($"{nameof(Member)}.{nameof(Member.Email)}", $"More than one active user found with email '{Member.Email}'");
            }

            return Page();
        }
    }
}