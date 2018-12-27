using System.Threading.Tasks;
using Ether.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ether.Vsts.Config
{
    public class VstsInitialMigration : IMigration
    {
        private readonly IRepository _repository;
        private readonly ILogger<VstsInitialMigration> _logger;

        public VstsInitialMigration(IRepository repository, ILogger<VstsInitialMigration> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Run()
        {
            await Task.Yield();

            // var isCreated = await _repository.CreateOrUpdateIfAsync(s => s.Type == Constants.VstsType, new VstsDataSourceSettings());
            // var message = isCreated ? "created" : "already exists";
            // _logger.LogInformation($"{nameof(VstsDataSourceSettings)} record {message}");
        }
    }
}
