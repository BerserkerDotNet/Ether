using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ether.EmailGenerator
{
    public class Worker : BackgroundService
    {
        private readonly EmailGeneratorService _service;
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<ServiceConfig> _serviceConfig;

        public Worker(EmailGeneratorService service, IOptions<ServiceConfig> serviceConfig, ILogger<Worker> logger)
        {
            _service = service;
            _serviceConfig = serviceConfig;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = _serviceConfig.Value;
            var server = new Server
            {
                Services = { EmailGenerator.BindService(_service) },
                Ports = { new ServerPort(config.Host, config.Port, ServerCredentials.Insecure) }
            };

            server.Start();

            _logger.LogInformation("Email Generator is running on {Host}:{Port}.", config.Host, config.Port);
            await stoppingToken;
            _logger.LogWarning("Cancellation received. Shutting down Email Generator.");
            await server.ShutdownAsync();
        }
    }
}
