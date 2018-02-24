using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Proxy
{
    public class PullRequestsInterceptor : IInterceptor
    {
        private const string IterationsName = "get_Iterations";
        private const string ThreadsName = "get_Threads";
        private static string[] _supportedMethods = new[] { IterationsName , ThreadsName };

        private readonly IVstsClientRepository _vstsRepository;
        private readonly ILogger<PullRequestsInterceptor> _logger;
        private IEnumerable<PullRequestIteration> _iterations;
        private IEnumerable<PullRequestThread> _threads;

        public PullRequestsInterceptor(IVstsClientRepository vstsRepository, ILogger<PullRequestsInterceptor> logger)
        {
            _vstsRepository = vstsRepository;
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (invocation.InvocationTarget == null || invocation.TargetType != typeof(PullRequest) || !CanHandle(methodName))
            {
                invocation.Proceed();
                return;
            }

            var pullRequest = (PullRequest)invocation.InvocationTarget;
            invocation.ReturnValue = GetData(methodName, pullRequest);
        }

        private object GetData(string methodName, PullRequest pullRequest)
        {
            switch (methodName)
            {
                case IterationsName:
                    return GetIterations(pullRequest);
                case ThreadsName:
                    return GetThreads(pullRequest);
                default:
                    throw new NotSupportedException($"Method with name '{methodName}' is not supported.");
            }
        }

        private object GetIterations(PullRequest pullRequest)
        {
            if (_iterations == null)
            {
                var repositoryName = pullRequest.Repository.Name;
                var projectName = pullRequest.Repository.Project.Name;
                _iterations = _vstsRepository.GetIterations(projectName, repositoryName, pullRequest.PullRequestId)
                    .GetAwaiter()
                    .GetResult();
            }

            return _iterations;
        }

        private object GetThreads(PullRequest pullRequest)
        {
            if (_threads == null)
            {
                var repositoryName = pullRequest.Repository.Name;
                var projectName = pullRequest.Repository.Project.Name;
                _threads = _vstsRepository.GetThreads(projectName, repositoryName, pullRequest.PullRequestId)
                    .GetAwaiter()
                    .GetResult();
            }

            return _threads;
        }

        private bool CanHandle(string name)
        {
            return _supportedMethods.Contains(name);
        }
    }
}
