using System.Collections.Generic;
using Ether.Redux.Interfaces;
using Ether.ViewModels;

namespace Ether.Actions
{
    public class ReceiveProfilesAction : IAction
    {
        public IEnumerable<ProfileViewModel> Profiles { get; set; }
    }
}
