using Castle.DynamicProxy;
using Ether.Core.Models.VSTS;

namespace Ether.Core.Proxy
{
    public class PullRequestsInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.TargetType != typeof(PullRequest))
            {
                invocation.Proceed();
                return;
            }
        }
    }
}
