using Blazor.Extensions.Storage;
using Ether.Types;
using Microsoft.AspNetCore.Blazor.Builder;
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
            services.AddStorage();
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
