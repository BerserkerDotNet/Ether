using System;

namespace Ether.Api.Jobs
{
    public class JobConfiguration
    {
        public JobConfiguration(Type jobType, TimeSpan period)
        {
            JobType = jobType;
            Period = period;
        }

        public Type JobType { get; }

        public TimeSpan Period { get; }
    }
}
