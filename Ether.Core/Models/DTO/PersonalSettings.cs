using System;

namespace Ether.Core.Models.DTO
{
    public class PersonalSettings : BaseDto
    {
        public Guid MyTeamProfile { get; set; }

        public string Owner { get; set; }
    }
}
