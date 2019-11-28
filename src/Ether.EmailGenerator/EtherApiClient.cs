using Ether.ViewModels;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ether.EmailGenerator
{
    public class EtherApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string _appUrl;

        public EtherApiClient()
        {
            _appUrl = ConfigurationManager.AppSettings.Get("AppUrl");
        }

        public async Task<string> GetToken()
        {
            var id = ConfigurationManager.AppSettings.Get("ClientId");
            var secret = ConfigurationManager.AppSettings.Get("ClientSecret");

            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"{_appUrl}connect/token",
                ClientId = id,
                ClientSecret = secret,
                Scope = "api"
            }).ConfigureAwait(false);

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.ErrorDescription);
            }

            return tokenResponse.AccessToken;
        }

        public void SetAccessToken(string token)
        {
            _httpClient.SetBearerToken(token);
        }

        public Task<IEnumerable<ProfileViewModel>> GetProfiles()
        {
            return HttpGet<IEnumerable<ProfileViewModel>>($"{_appUrl}vsts/profile/GetAll");
        }

        public Task<IEnumerable<TeamMemberViewModel>> GetTeamMembers()
        {
            return HttpGet<IEnumerable<TeamMemberViewModel>>($"{_appUrl}vsts/teammember/GetAll");
        }

        public async Task<WorkItemsReportViewModel> GenerateWorkItemsReport(Guid profileId)
        {
            var end = DateTime.Today.AddDays(1).AddSeconds(-1);
            var reportRequest = new GenerateReportViewModel
            {
                Profile = profileId,
                ReportType = "WorkitemsReporter",
                Start = end.AddDays(-7),
                End = end
            };

            var reportGuid = await HttpPost<Guid>($"{_appUrl}report/Generate", reportRequest)
                .ConfigureAwait(false);

            var report = await HttpGet<WorkItemsReportViewModel>($"{_appUrl}report/GetById?id={reportGuid}")
                .ConfigureAwait(false);

            return report;
        }

        private async Task<T> HttpGet<T>(string url)
        {
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            VerifyResponseStatusCode(response);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task<T> HttpPost<T>(string url, object payload)
        {
            var response = await HttpPostInternal(url, payload).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> HttpPostInternal(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            VerifyResponseStatusCode(response);

            return response;
        }

        private void VerifyResponseStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");
            }
        }
    }
}
