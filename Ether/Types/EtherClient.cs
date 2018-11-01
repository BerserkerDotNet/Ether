using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ether.ViewModels;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Http;

namespace Ether.Types
{
    public class EtherClient
    {
        private readonly HttpClient _httpClient;

        public EtherClient(HttpClient httpClient)
        {
            BrowserHttpMessageHandler.DefaultCredentials = FetchCredentialsOption.Include;
            _httpClient = httpClient;
            httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
        }

        public Task<string> GetCurrentUserNameAsync()
        {
            return _httpClient.GetStringAsync("User/Name");
        }

        public async Task<bool> IsUserHasAccess(string path, string category)
        {
            var result = await _httpClient.GetStringAsync($"User/HasMenuAccess?path={path}&category={category}");
            return bool.Parse(result);
        }

        public Task<VstsDataSourceViewModel> GetVstsDataSourceConfig()
        {
            return _httpClient.GetJsonAsync<VstsDataSourceViewModel>("Settings/VstsDataSourceConfiguration");
        }
    }
}
