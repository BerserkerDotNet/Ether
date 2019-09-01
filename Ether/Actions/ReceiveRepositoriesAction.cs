using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveRepositoriesAction : IAction
    {
        public IEnumerable<VstsRepositoryViewModel> Repositories { get; set; }
    }
}
