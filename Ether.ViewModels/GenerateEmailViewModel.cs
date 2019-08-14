using System;
using System.Collections.Generic;

namespace Ether.ViewModels
{
    public class GenerateEmailViewModel
    {
        public Guid Id { get; set; }

        public string Points { get; set; }

        public IEnumerable<TeamAttendanceViewModel> Attendance { get; set; }
    }
}
