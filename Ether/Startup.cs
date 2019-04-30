using Ether.Types;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
