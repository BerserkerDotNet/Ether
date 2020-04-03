using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types
{
    public class AppHealthStatus
    {
        public string Status { get; set; }

        public Dictionary<string, HealthCheckStatus> Results { get; set; }

    }

    public class HealthCheckStatus
    {
        public string Status { get; set; }

        public string Description { get; set; }
    }
}
