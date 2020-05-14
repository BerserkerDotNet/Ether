using BlazorState.Redux.Interfaces;
using Ether.ViewModels;
using System.Collections.Generic;

namespace Ether.Actions
{
    public class ReceiveOrganizationsAction : IAction
    {
        public IEnumerable<OrganizationViewModel> Organizations { get; set; }
    }
}
