using Ether.Core.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ether.Core.Proxy;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Data
{
    public class VSTSClient : IVSTSClient
    {
        private const string JsonMimeType = "application/json";
        private readonly HttpClient _client;
        private readonly IDependencyResolver _resolver;
        private readonly ILogger<VSTSClient> _logger;

        public VSTSClient(HttpClient client, IDependencyResolver resolver, ILogger<VSTSClient> logger)
        {
            _client = client;
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<T> ExecuteGet<T>(string url)
        {
            try
            {
                using (var response = await _client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
                    return JsonConvert.DeserializeObject<T>(resultContent, converter);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occured while requesting {APIUrl}", url);
                return default(T);
            }
        }

        public Task<T> ExecutePost<T>(string url, object payload)
        {
            var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
            return ExecutePost<T>(url, JsonConvert.SerializeObject(payload, converter));
        }

        public async Task<T> ExecutePost<T>(string url, string payload)
        {
            try
            {
                var content = new StringContent(payload, Encoding.UTF8, JsonMimeType);
                using (var response = await _client.PostAsync(url, content))
                {
                    response.EnsureSuccessStatusCode();
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
                    return JsonConvert.DeserializeObject<T>(resultContent, converter);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occured while posting to {APIUrl}", url);
                return default(T);
            }
        }
    }
}
