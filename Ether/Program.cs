using System;
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
#if DEBUG
                .UseHttpSys(options=> {
                    options.Authentication.Schemes = AuthenticationSchemes.NTLM;
                    options.Authentication.AllowAnonymous = true;
                    options.MaxConnections = 100;
                    options.MaxRequestBodySize = 30000000;
                })
#endif
                .UseStartup<Startup>();
    }
}
