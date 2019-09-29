using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ether.EmailGenerator
{
    public static class CancellationTokenExtensions
    {
        public static CancellationTokenAwaiter GetAwaiter(this CancellationToken cancellationToken)
        {
            return new CancellationTokenAwaiter(cancellationToken);
        }

        public class CancellationTokenAwaiter : INotifyCompletion
        {
            private readonly CancellationToken _cancellationToken;

            public CancellationTokenAwaiter(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
            }

            public bool IsCompleted
            {
                get
                {
                    return _cancellationToken.IsCancellationRequested;
                }
            }

            public void GetResult()
            {
            }

            public void OnCompleted(Action action)
            {
                _cancellationToken.Register(action);
            }
        }
    }
}
