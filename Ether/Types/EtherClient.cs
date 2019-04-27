using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ether.ViewModels;
using IdentityModel.Client;
using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ether.Types
{
    public class EtherClient
    {
        // TODO: Better mapping
        private static readonly Dictionary<Type, string> _typeRoutes = new Dictionary<Type, string>(9)
        {
            { typeof(VstsProjectViewModel), "vsts/project" },
            { typeof(VstsRepositoryViewModel), "vsts/repository" },
            { typeof(TeamMemberViewModel), "vsts/teammember" },
            { typeof(ProfileViewModel), "vsts/profile" },
            { typeof(IdentityViewModel), "identity" },
            { typeof(ReportViewModel), "report" },
            { typeof(PullRequestReportViewModel), "report" },
            { typeof(AggregatedWorkitemsETAReportViewModel), "report" },
            { typeof(WorkItemsReportViewModel), "report" },
            { typeof(GenerateReportViewModel), "report" },
            { typeof(ReporterDescriptorViewModel), "report" },
            { typeof(JobLogViewModel), "jobs/logs" }
        };

        private readonly HttpClient _httpClient;
        private readonly IUriHelper _navigation;

        public EtherClient(HttpClient httpClient, IUriHelper navigation)
        {
            WebAssemblyHttpMessageHandler.DefaultCredentials = FetchCredentialsOption.Include;
            _httpClient = httpClient;
            this._navigation = navigation;
            httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
        }

        public void SetAccessToken(string token)
        {
            _httpClient.SetBearerToken(token);
        }

        public async Task<UserViewModel> GetCurrentUserAsync()
        {
            return await HttpGet<UserViewModel>("User/GetUser");
        }

        public async Task<bool> IsUserHasAccess(string path, string category)
        {
            try
            {
                return await HttpGet<bool>($"User/HasMenuAccess?path={path}&category={category}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public Task<VstsDataSourceViewModel> GetVstsDataSourceConfig()
        {
            return HttpGet<VstsDataSourceViewModel>("Settings/VstsDataSourceConfiguration");
        }

        public Task SaveVstsDataSourceConfig(VstsDataSourceViewModel model)
        {
            return HttpPost("Settings/VstsDataSourceConfiguration", model);
        }

        public Task<IEnumerable<T>> GetAll<T>()
        {
            return HttpGet<IEnumerable<T>>($"{GetPathFor<T>()}/GetAll");
        }

        public Task<T> GetById<T>(Guid id)
        {
            return HttpGet<T>($"{GetPathFor<T>()}/GetById?id={id}");
        }

        public Task Save<T>(T model)
        {
            return HttpPost($"{GetPathFor<T>()}/Save", model);
        }

        public Task Delete<T>(Guid id)
        {
            return HttpDelete($"{GetPathFor<T>()}/Delete?id={id}");
        }

        public Task<Guid> GenerateReport(GenerateReportViewModel model)
        {
            return HttpPost<Guid>($"{GetPathFor<GenerateReportViewModel>()}/Generate", model);
        }

        public Task RunWorkitemsJob(IEnumerable<Guid> members, bool isReset)
        {
            return HttpPost("jobs/vsts/runworkitemsjob", new { Members = members, IsReset = isReset });
        }

        public Task<IEnumerable<ReporterDescriptorViewModel>> GetReportTypes()
        {
            return HttpGet<IEnumerable<ReporterDescriptorViewModel>>($"{GetPathFor<ReporterDescriptorViewModel>()}/types");
        }

        public async Task<AccessToken> RequestAccessToken(LoginViewModel model)
        {
            var tokenResponse = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = "http://localhost:5000/connect/token",
                ClientId = "EtherBlazorClient",
                UserName = model.UserName,
                Password = model.Password,
                Scope = "api"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                throw new Exception(tokenResponse.Error);
            }

            SetAccessToken(tokenResponse.AccessToken);
            return new AccessToken(tokenResponse.AccessToken, TimeSpan.FromSeconds(tokenResponse.ExpiresIn));
        }

        private async Task<T> HttpGet<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            VerifyResponseStatusCode(response);

            var content = await response.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(content);
        }

        private Task HttpPost(string url, object payload)
        {
            return HttpPostInternal(url, payload);
        }

        private async Task<T> HttpPost<T>(string url, object payload)
        {
            var response = await HttpPostInternal(url, payload);
            var content = await response.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(content);
        }

        private async Task<HttpResponseMessage> HttpPostInternal(string url, object payload)
        {
            var json = Json.Serialize(payload);
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            VerifyResponseStatusCode(response);

            return response;
        }

        private async Task HttpDelete(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            VerifyResponseStatusCode(response);
        }

        private void VerifyResponseStatusCode(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _navigation.NavigateTo(Routes.LoginExpiredSession);
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");
            }
        }

        private string GetPathFor<T>()
        {
            var key = typeof(T);
            return _typeRoutes.ContainsKey(typeof(T)) ? _typeRoutes[key] : string.Empty;
        }
    }
}
