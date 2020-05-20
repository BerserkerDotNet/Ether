using System;
using System.Threading.Tasks;
using BlazorState.Redux.Interfaces;
using Ether.Actions;

namespace Ether.Types
{
    public static class Utils
    {
        public static async Task ExecuteWithLoading(IDispatcher dispatcher, Func<Task> action)
        {
            try
            {
                dispatcher.Dispatch(new Loading());
                await action();
            }
            finally
            {
                dispatcher.Dispatch(new Loading { IsDone = true });
            }
        }
    }
}
