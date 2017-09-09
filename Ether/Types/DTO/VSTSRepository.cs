using System;

namespace Ether.Types.DTO
{
    public class VSTSRepository : BaseDto
    {
        public string Name { get; set; }

        public Guid Project { get; set; }
    }
}
