using System.Collections.Generic;
using BlazorState.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveProfilesAction : IAction
    {
        public IEnumerable<ProfileViewModel> Profiles { get; set; }
    }
}
