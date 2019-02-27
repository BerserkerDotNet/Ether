using System;
using Ether.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ether.Api.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJobs(this IServiceCollection services, Action<JobsConfigurator> config)
        {
            services.AddHostedService<JobsHostedService>();
            services.AddSingleton<IJobRunner>(p => (JobsHostedService)p.GetService<IHostedService>());
            config?.Invoke(new JobsConfigurator(services));
        }
    }
}
