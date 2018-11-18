using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ether.Api.Jobs
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJobs(this IServiceCollection services, Action<JobsConfigurator> config)
        {
            services.AddHostedService<JobsHostedService>();
            config?.Invoke(new JobsConfigurator(services));
        }
    }
}
