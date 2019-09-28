using System;
using System.Collections.Generic;

namespace Ether.Actions.Async
{
    public class FetchDataJobParameters
    {
        public IEnumerable<Guid> Members { get; set; }

        public bool Reset { get; set; }
    }
}
