//using Blazor.Extensions.Logging;
using Ether.Components.Modal;
using Ether.Types;
using Ether.Types.EditableTable;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddSingleton<AppState>();
            services.AddSingleton<EtherClientEditableTableDataProvider>();
            services.AddSingleton<NoOpEditableTableDataProvider>();
            services.AddScoped<ModalService>();

            //services.AddLogging(builder => builder
            //    .AddBrowserConsole()
            //    .SetMinimumLevel(LogLevel.Trace)
            //);
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
