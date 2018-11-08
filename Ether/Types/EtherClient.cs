using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ether.ViewModels;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Http;

namespace Ether.Types
{
    public class EtherClient
    {
        private static readonly Dictionary<Type, string> _typeRoutes = new Dictionary<Type, string>(5)
        {
            { typeof(VstsProjectViewModel), "vsts/project" },
            { typeof(VstsRepositoryViewModel), "vsts/repository" },
            { typeof(VstsTeamMemberViewModel), "vsts/teammember" },
            { typeof(VstsProfileViewModel), "vsts/profile" },
            { typeof(IdentityViewModel), "identity" }
        };

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

        public Task SaveVstsDataSourceConfig(VstsDataSourceViewModel model)
        {
            return _httpClient.PostJsonAsync("Settings/VstsDataSourceConfiguration", model);
        }

        public Task<IEnumerable<T>> GetAll<T>()
        {
            return _httpClient.GetJsonAsync<IEnumerable<T>>($"{GetPathFor<T>()}/GetAll");
        }

        public Task Save<T>(T model)
        {
            return _httpClient.PostJsonAsync($"{GetPathFor<T>()}/Save", model);
        }

        public Task Delete<T>(Guid id)
        {
            return _httpClient.DeleteAsync($"{GetPathFor<T>()}/Delete?id={id}");
        }

        private string GetPathFor<T>()
        {
            var key = typeof(T);
            return _typeRoutes.ContainsKey(typeof(T)) ? _typeRoutes[key] : string.Empty;
        }
    }
}
