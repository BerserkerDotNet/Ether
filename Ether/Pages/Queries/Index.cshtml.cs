using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Ether.Pages.Queries
{
    [PageTitle("Queries")]
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
            Queries = _repository.GetAll<Query>();
        }
    }
}