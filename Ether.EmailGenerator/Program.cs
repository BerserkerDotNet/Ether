using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ether.EmailGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
#if !DEBUG
                .UseWindowsService()
#endif
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<EmailGeneratorService>();
                    services.Configure<ServiceConfig>(hostContext.Configuration.GetSection("Service"));
                    services.AddHostedService<Worker>();
                    services.AddLogging(builder =>
                    {
                        builder.AddSeq();
                    });
                });
    }
}
