using Ether.ViewModels;
using System.Collections.Generic;

namespace Ether.Types.State
{
    public class RepositoriesState
    {
        public RepositoriesState(IEnumerable<VstsRepositoryViewModel> repositories)
        {
            Repositories = repositories;
        }

        public IEnumerable<VstsRepositoryViewModel> Repositories { get; private set; }
    }
}
