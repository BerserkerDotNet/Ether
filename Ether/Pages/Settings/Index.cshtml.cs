using Ether.Core.Interfaces;
using Ether.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Ether.Pages.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;

        public IndexModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public Ether.Core.Models.DTO.Settings Settings { get; set; }

        public async Task OnGetAsync()
        {
            Settings = await _repository.GetSingleAsync<Ether.Core.Models.DTO.Settings>(_ => true);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _repository.CreateOrUpdateAsync(Settings);
            TempData.WithSuccess("Settings were saved.");

            return RedirectToPage("/Settings/Index");
        }
    }
}