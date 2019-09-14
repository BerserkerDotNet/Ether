using System.Collections.Generic;
using Ether.ViewModels;
using Newtonsoft.Json;

namespace Ether.Types.State
{
    public class RepositoriesState
    {
        [JsonConstructor]
        public RepositoriesState(IEnumerable<VstsRepositoryViewModel> repositories)
        {
            Repositories = repositories;
        }

        public IEnumerable<VstsRepositoryViewModel> Repositories { get; private set; }
    }
}
