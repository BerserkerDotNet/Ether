using System;
using System.Collections.Generic;

namespace Ether.Types
{
    public class AppHealthStatus
    {
        public string Status { get; set; }

        public HealthStatus HealthStatus => (HealthStatus)Enum.Parse(typeof(HealthStatus), Status);

        public Dictionary<string, HealthCheckStatus> Results { get; set; }
    }

    public class HealthCheckStatus
    {
        public string Status { get; set; }

        public HealthStatus HealthStatus => (HealthStatus)Enum.Parse(typeof(HealthStatus), Status);

        public string Description { get; set; }
    }

    public enum HealthStatus
    {
        Unhealthy,
        Degraded,
        Healthy
    }
}
