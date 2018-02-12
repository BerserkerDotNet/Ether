using System.Collections.Generic;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Ether.Pages.Queries
{
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;

        public IEnumerable<Query> Queries { get; set; }

        public IndexModel(IRepository repository, ILogger<Query> logger)
        {
            _repository = repository;
        }

        public void OnGet()
        {
            ViewData["CurrentMenu"] = "Queries";
            Queries = _repository.GetAll<Query>();
        }
    }
}