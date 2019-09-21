//using Blazor.Extensions.Logging;
using Blazor.Extensions.Storage;
using BlazorBootstrap.Modal;
using BlazorState.Redux.Extensions;
using BlazorState.Redux.Storage;
using Ether.Actions.Async;
using Ether.Reducers;
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
                cfg.UseReduxDevTools();
                cfg.UseLocalStorage();
                cfg.TrackUserNavigation(s => s.Location);

                cfg.Map<JobLogsReducer, JobLogsState>(s => s.JobLogs);
                cfg.Map<ProfilesReducer, ProfilesState>(s => s.Profiles);
                cfg.Map<GenerateReportFormReducer, GenerateReportFormState>(s => s.GenerateReportForm);
                cfg.Map<SettingsReducer, SettingsState>(s => s.Settings);
                cfg.Map<ReportsReducer, ReportsState>(s => s.Reports);
                cfg.Map<MembersReducer, TeamMembersState>(s => s.TeamMembers);
                cfg.Map<RepositoriesReducer, RepositoriesState>(s => s.Repositories);
                cfg.Map<ProjectsReducer, ProjectsState>(s => s.Projects);

                cfg.RegisterActionsFromAssemblyContaining<FetchProfiles>();
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
