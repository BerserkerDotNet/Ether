using Ether.Types.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ether.Types.Data
{
    public class VSTSClient
    {
        private const string JsonMimeType = "application/json";
        private readonly VSTSConfiguration _configuration;

        public VSTSClient(IOptions<VSTSConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        public async Task<string> ExecuteGet(string url)
        {
            using (var client = new HttpClient())
            {
                AddHeaders(client);
                using (var response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
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
