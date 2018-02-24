using System;
using Castle.DynamicProxy;
using Ether.Core.Interfaces;
using Ether.Core.Models.VSTS;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ether.Core.Proxy
{
    public class PullRequestProxyJsonConverter : JsonConverter
    {
        private readonly ProxyGenerator _generator;
        private readonly IVstsClientRepository _vstsRepository;
        private readonly ILogger<PullRequestsInterceptor> _interceptorLogger;

        public PullRequestProxyJsonConverter(ProxyGenerator generator, IVstsClientRepository vstsRepository, ILogger<PullRequestsInterceptor> logger)
        {
            _generator = generator;
            _vstsRepository = vstsRepository;
            _interceptorLogger = logger;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PullRequest);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Since interceptor holds state related to a specific PullRequest object need to make sure wwe create a new instance for every proxy.
            var interceptor = new PullRequestsInterceptor(_vstsRepository, _interceptorLogger);
            var prProxy = _generator.CreateClassProxy<PullRequest>(interceptor);
            serializer.Populate(reader, prProxy);
            return prProxy;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(PullRequestProxyJsonConverter)} only supports read operation.");
        }

        public override bool CanWrite => false;
    }
}
