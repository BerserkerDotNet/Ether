using System;

namespace Ether.Redux.Blazor
{
    public class JumpToStateEventArgs : EventArgs
    {
        public JumpToStateEventArgs(string stateJson)
        {
            StateJson = stateJson;
        }

        public string StateJson { get; private set; }
    }
}
