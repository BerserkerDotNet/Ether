using FluentScheduler;
using System;

namespace Ether.Jobs
{
    public class DIFriendlyJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DIFriendlyJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _serviceProvider.GetService(typeof(T)) as IJob;
        }
    }
}
