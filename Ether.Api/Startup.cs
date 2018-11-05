using System;
using Autofac;
using AutoMapper;
using Ether.Api.Types;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Types.Configuration;
using Ether.Core.Config;
using Ether.Vsts.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

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
            services.AddOptions();
            services.Configure<DbConfiguration>(Configuration.GetSection("DbConfig"));

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new CoreMappingProfile());
                mc.AddProfile(new VstsMappingProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddResponseCompression();
            services.AddIdentityServer(o =>
            {
                o.UserInteraction.LoginUrl = "/login/challenge";
            })
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());

            services.AddAuthentication();

            services.AddCors();
            services.AddMvc(o =>
            {
                o.Conventions.Add(new NotFoundOnNullResultFilterConvention());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Ether API", Version = "v1" });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<EtherCoreModule>();
            builder.RegisterModule<VstsModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RunMigrations(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseCors(b => b.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod()
                             .AllowCredentials());

            app.UseIdentityServer();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ether API V1");
            });

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
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
