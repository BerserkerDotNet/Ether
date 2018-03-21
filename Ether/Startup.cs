using Castle.DynamicProxy;
using Ether.Core.Configuration;
using Ether.Core.Data;
using Ether.Core.Filters;
using Ether.Core.Interfaces;
using Ether.Core.Proxy;
using Ether.Core.Reporters;
using Ether.Core.Reporters.Classifiers;
using Ether.Hubs;
using Ether.Jobs;
using Ether.Services;
using FluentScheduler;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Headers;
using System.Text;
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
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                o.Filters.Add(new AuthorizeFilter(policy));
                o.Filters.Add<CurrentMenuIndicatorFilter>();
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
            .AddRazorPagesOptions(options => 
            {
                options.Conventions.AddPageRoute("/Home/Index", "");
            })
            .SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.AddSignalR();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRepository, MongoRepository>();
            services.AddSingleton<IVstsClientRepository, VstsClientRepository>();
            services.AddSingleton<DIFriendlyJobFactory>();
            services.AddSingleton<ProxyGenerator>();
            services.AddSingleton<IDependencyResolver, AspNetDependencyResolver>();
            services.AddSingleton<PullRequestProxyJsonConverter>();
            services.AddSingleton<IProgressReporter, SignalRProgressReporter>();

            services.AddScoped(typeof(IAll<>), typeof(DataManager<>));
            services.AddScoped<IReporter, PullRequestsReporter>();
            services.AddScoped<IReporter, WorkItemsReporter>();
            services.AddScoped<IReporter, ListOfReviewersReporter>();
            services.AddScoped<IWorkItemsClassifier, ResolvedWorkItemsClassifier>();
            services.AddScoped<IWorkItemsClassifier, ClosedTasksWorkItemsClassifier>();
            services.AddScoped<LiveUpdatesHub>();

            services.AddTransient<WorkItemsFetchJob>();
            services.AddTransient<PullRequestsFetchJob>();
            services.AddTransient<RetentionJob>();
#if DEBUG
            services.AddAuthentication(Microsoft.AspNetCore.Server.HttpSys.HttpSysDefaults.AuthenticationScheme);
#endif
            services.AddHttpClient<IVSTSClient, VSTSClient>(client => 
            {
                var vstsConfig = _configuration.Get<VSTSConfiguration>();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", vstsConfig.AccessToken)));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", parameter);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeJobs(app);

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

            app.UseSignalR(routes => 
            {
                routes.MapHub<LiveUpdatesHub>("/liveupdates");
            });

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private static void InitializeJobs(IApplicationBuilder app)
        {
            var registry = new Registry();
            registry.Schedule<WorkItemsFetchJob>()
                .ToRunNow()
                .AndEvery(1)
                .Hours();
            registry.Schedule<PullRequestsFetchJob>()
                .ToRunNow()
                .AndEvery(1)
                .Days();
            registry.Schedule<RetentionJob>()
                .ToRunNow()
                .AndEvery(1)
                .Days();
            JobManager.JobFactory = app.ApplicationServices.GetService<DIFriendlyJobFactory>();
            JobManager.Initialize(registry);
        }
    }
}
