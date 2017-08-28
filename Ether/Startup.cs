using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.Data;
using Ether.Types.Reporters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ether
{
    public class Startup
    {
        IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables("EtherReport_")
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<VSTSConfiguration>(_configuration);
            services.AddResponseCompression();
            services.AddMvc();
            services.AddSingleton(typeof(IRepository), typeof(FileRepository));
            services.AddScoped<VSTSClient>();
            services.AddScoped<PullRequestsReporter>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseStatusCodePages(async context =>
                {
                    context.HttpContext.Response.ContentType = "text/plain";
                    await context.HttpContext.Response.WriteAsync(
                        "Status code page, status code: " +
                        context.HttpContext.Response.StatusCode);
                });
            }

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
