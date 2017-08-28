using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ether.Models
{
    public class ReportViewModel
    {
        [Required]
        public Guid Profile { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }

        public IEnumerable<SelectListItem> Profiles { get; set; }
    }
}
