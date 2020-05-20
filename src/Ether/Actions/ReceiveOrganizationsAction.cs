using System.Collections.Generic;
using BlazorState.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveOrganizationsAction : IAction
    {
        public IEnumerable<OrganizationViewModel> Organizations { get; set; }
    }
}
