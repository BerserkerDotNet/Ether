using System.Collections.Generic;
using BlazorState.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveIdentitiesAction : IAction
    {
        public IEnumerable<IdentityViewModel> Identities { get; set; }
    }
}
