using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#if DEBUG
using Microsoft.AspNetCore.Server.HttpSys;
#endif

namespace Ether
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
