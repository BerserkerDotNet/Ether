using System;

namespace Ether.Models
{
    public class TeamMemberViewModel
    {
        public string DisplayName { get; set; }

        public string Email { get; set; }

        public Guid? Id { get; set; }

        public string TeamName { get; set; }
    }
}
