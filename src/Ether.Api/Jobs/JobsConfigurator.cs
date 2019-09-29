using System;
using Ether.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ether.Api.Jobs
{
    public class JobsConfigurator
    {
        private readonly IServiceCollection _services;

        public JobsConfigurator(IServiceCollection services)
        {
            this._services = services;
        }

        public void RecurrentJob<T>(TimeSpan period)
            where T : class, IJob
        {
            _services.AddSingleton<T>();
            _services.AddSingleton(new JobConfiguration(typeof(T), period));
        }
    }
}
