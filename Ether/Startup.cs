using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.Data;
using Ether.Types.Filters;
using Ether.Types.Reporters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ether
{
    public class Startup
    {
        IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables("EtherReport_")
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                var config = _configuration.GetSection("Seq");
                loggingBuilder.AddSeq(config);
            });
            services.AddOptions();
            services.Configure<VSTSConfiguration>(_configuration);
            services.Configure<DbConfig>(_configuration.GetSection("DbConfig"));
            services.AddResponseCompression();
            services.AddMvc(o =>
            {
                o.Filters.Add<CurrentMenuIndicatorFilter>();
            }).AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
            services.AddSingleton<IRepository, MongoRepository>();
            services.AddScoped<IVSTSClient, VSTSClient>();
            services.AddScoped(typeof(IAll<>), typeof(DataManager<>));
            services.AddScoped<IReporter, PullRequestsReporter>();
            services.AddScoped<IReporter, WorkItemsReporter>();
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
