using System;

namespace Ether.Contracts.Dto
{
    public class Organization : BaseDto
    {
        public Organization(string type)
        {
            Type = type;
        }

        public Guid? Identity { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
