using System;

namespace Ether.ViewModels
{
    public class TeamMemberViewModel : ViewModelWithId
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public DateTime? LastWorkitemsFetchDate { get; set; }
    }
}
