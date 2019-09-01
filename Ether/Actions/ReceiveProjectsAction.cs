using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveProjectsAction : IAction
    {
        public IEnumerable<VstsProjectViewModel> Projects { get; set; }
    }
}
