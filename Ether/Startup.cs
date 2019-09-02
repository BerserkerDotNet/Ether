//using Blazor.Extensions.Logging;
using BlazorBootstrap.Modal;
using Ether.Actions.Async;
using Ether.Reducers;
using Ether.Redux.Extensions;
using Ether.Types;
using Ether.Types.EditableTable;
using Ether.Types.State;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Ether
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<EtherClient>();
            services.AddSingleton<EtherMenuService>();
            services.AddSingleton<ModelValidationService>();
            services.AddSingleton<TokenService>();
            services.AddSingleton<JsUtils>();
            services.AddSingleton<LocalStorage>();

            // State
            services.AddReduxStore<RootState>(cfg =>
            {
                // TODO: map to reducer type, e.g. cfg.Map<JobLogsReducer>(s => s.JobLogs)
                cfg.Map(s => s.JobLogs, new JobLogsReducer());
                cfg.Map(s => s.Profiles, new ProfilesReducer());
                cfg.Map(s => s.GenerateReportForm, new GenerateReportFormReducer());
                cfg.Map(s => s.Settings, new SettingsReducer());
                cfg.Map(s => s.Reports, new ReportsReducer());
                cfg.Map(s => s.TeamMembers, new MembersReducer());
                cfg.Map(s => s.Repositories, new RepositoriesReducer());
                cfg.Map(s => s.Projects, new ProjectsReducer());

                cfg.RegisterActionFromAssembly<FetchProfiles>();
            });

            services.AddSingleton<EtherClientEditableTableDataProvider>();
            services.AddSingleton<NoOpEditableTableDataProvider>();
            services.AddBootstrapModal();

            //services.AddLogging(builder => builder
            //    .AddBrowserConsole()
            //    .SetMinimumLevel(LogLevel.Trace)
            //);
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.UseLocalTimeZone();
            app.AddComponent<App>("app");
        }
    }
}
