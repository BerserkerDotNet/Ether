using System;
using System.Collections.Generic;

namespace Ether.Types.DTO
{
    public class Profile : BaseDto
    {
        public string Name { get; set; }

        public IEnumerable<Guid> Repositories { get; set; }

        public IEnumerable<Guid> Members { get; set; }
    }
}
