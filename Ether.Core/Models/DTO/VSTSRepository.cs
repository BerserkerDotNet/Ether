using System;

namespace Ether.Core.Models.DTO
{
    public class VSTSRepository : BaseDto
    {
        public string Name { get; set; }

        public Guid Project { get; set; }
    }
}
