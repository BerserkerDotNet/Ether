using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Ether.Core.Proxy;

namespace Ether.Core.Data
{
    public class VSTSClient : IVSTSClient
    {
        private const string JsonMimeType = "application/json";
        private readonly VSTSConfiguration _configuration;
        private readonly IDependencyResolver _resolver;

        public VSTSClient(IOptions<VSTSConfiguration> configuration, IDependencyResolver resolver)
        {
            _configuration = configuration.Value;
            _resolver = resolver;
        }

        public async Task<T> ExecuteGet<T>(string url)
        {
            using (var client = new HttpClient())
            {
                AddHeaders(client);
                using (var response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
                    return JsonConvert.DeserializeObject<T>(resultContent, converter);
                }
            }
        }

        public Task<T> ExecutePost<T>(string url, object payload)
        {
            var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
            return ExecutePost<T>(url, JsonConvert.SerializeObject(payload, converter));
        }

        public async Task<T> ExecutePost<T>(string url, string payload)
        {
            using (var client = new HttpClient())
            {
                AddHeaders(client);
                var content = new StringContent(payload, Encoding.UTF8, JsonMimeType);
                using (var response = await client.PostAsync(url, content))
                {
                    response.EnsureSuccessStatusCode();
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var converter = _resolver.Resolve<PullRequestProxyJsonConverter>();
                    return JsonConvert.DeserializeObject<T>(resultContent, converter);
                }
            }
        }

        private void AddHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _configuration.AccessToken))));
        }
    }
}
