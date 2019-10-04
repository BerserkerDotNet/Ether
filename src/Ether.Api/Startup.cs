using System;
using Autofac;
using AutoMapper;
using Ether.Api.Jobs;
using Ether.Api.Types;
using Ether.Api.Types.Email;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types.Configuration;
using Ether.Core.Config;
using Ether.Core.Extensions;
using Ether.Core.Jobs;
using Ether.Core.Types;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Ether.ViewModels.Validators;
using Ether.Vsts.Config;
using Ether.Vsts.Jobs;
using FluentValidation.AspNetCore;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ether.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddSeq();
            });

            services.AddOptions();
            services.Configure<DbConfiguration>(Configuration.GetSection("DbConfig"));
            services.Configure<ADConfiguration>(Configuration.GetSection("ADConfig"));
            services.Configure<EmailGeneratorConfiguration>(Configuration.GetSection("EmailGenerator"));

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new CoreMappingProfile());
                mc.AddProfile(new VstsMappingProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddResponseCompression();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
            {
                options.Authority = Configuration.GetValue<string>("Authority");
                options.RequireHttpsMetadata = false;
                options.ApiName = "api";
            });

            services.AddTransient<IResourceOwnerPasswordValidator, ADPasswordValidator>();
            services.AddTransient<IProfileService, ProfileServiceWithCustomClaims>();

            services.AddCors();
            services.AddMvc(o =>
            {
                o.Conventions.Add(new NotFoundOnNullResultFilterConvention());
            })
            .AddNewtonsoftJson()
            .SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IdentityViewModelValidator>());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ether API", Version = "v1" });
            });

            services.AddEmailGeneratorClient();

            services.AddReporter<GeneratePullRequestsReport, PullRequestsReport, PullRequestReportViewModel>(Constants.PullRequestsReportType, Constants.PullRequestsReportName);
            services.AddReporter<GenerateAggregatedWorkitemsETAReport, AggregatedWorkitemsETAReport, AggregatedWorkitemsETAReportViewModel>(Constants.ETAReportType, Constants.ETAReportName);
            services.AddReporter<GenerateWorkItemsReport, WorkItemsReport, WorkItemsReportViewModel>(Constants.WorkitemsReportType, Constants.WorkitemsReporterName);
            services.AddReporter<GenerateReOpenedWorkItemsReport, ReOpenedWorkItemsReport, ReOpenedWorkItemsReportViewModel>(Constants.ReOpenedWorkitemsReportType, Constants.ReOpenedWorkitemsReporterName);

            var jobsConfig = new JobsConfiguration();
            Configuration.GetSection("Jobs").Bind(jobsConfig);
            services.AddJobs(cfg =>
            {
                cfg.RecurrentJob<PullRequestsSyncJob>(TimeSpan.FromMinutes(jobsConfig.PullRequestJobRecurrence));
                cfg.RecurrentJob<WorkItemsSyncJob>(TimeSpan.FromMinutes(jobsConfig.WorkItemsJobRecurrence));
                cfg.RecurrentJob<RetentionJob>(TimeSpan.FromMinutes(jobsConfig.RetentionJobRecurrence));
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<EtherCoreModule>();
            builder.RegisterModule<VstsModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            RunMigrations(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseCors(b => b.WithOrigins("https://localhost:44316")
                             .AllowAnyHeader()
                             .AllowAnyMethod()
                             .AllowCredentials());
            }

            app.UseIdentityServer();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Ether API V1");
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RunMigrations(IApplicationBuilder app)
        {
            var migrations = app.ApplicationServices.GetServices<IMigration>();
            foreach (var migration in migrations)
            {
                // ToDo: This should be awaited
                migration.Run();
            }
        }
    }
}
