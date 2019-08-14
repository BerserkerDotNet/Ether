using System;

namespace Ether.ViewModels
{
    public class TeamAttendanceViewModel
    {
        public Guid MemberId { get; set; }

        public string MemberName { get; set; }

        public bool[] Attendance { get; set; }
    }
}
