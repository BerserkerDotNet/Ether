﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Ether.Types.Exceptions;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using IdentityModel.Client;
using MatBlazor;
using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ether.Types
{
    public partial class EtherClient
    {
        // TODO: Better mapping
        private static readonly Dictionary<Type, string> _typeRoutes = new Dictionary<Type, string>(9)
        {
            { typeof(VstsProjectViewModel), "vsts/project" },
            { typeof(VstsRepositoryViewModel), "vsts/repository" },
            { typeof(TeamMemberViewModel), "vsts/teammember" },
            { typeof(ProfileViewModel), "vsts/profile" },
            { typeof(IdentityViewModel), "identity" },
            { typeof(OrganizationViewModel), "organization" },
            { typeof(ReportViewModel), "report" },
            { typeof(PullRequestReportViewModel), "report" },
            { typeof(AggregatedWorkitemsETAReportViewModel), "report" },
            { typeof(WorkItemsReportViewModel), "report" },
            { typeof(ReOpenedWorkItemsReportViewModel), "report" },
            { typeof(GenerateReportViewModel), "report" },
            { typeof(ReporterDescriptorViewModel), "report" },
            { typeof(JobLogViewModel), "jobs/logs" },
            { typeof(PullRequestJobDetails), "jobs/logs" },
            { typeof(DashboardSettingsViewModel), "dashboard" }
        };

        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigation;
        private readonly IMatToaster _toaster;
        private readonly ILogger<EtherClient> _logger;

        public EtherClient(HttpClient httpClient, NavigationManager navigation, IMatToaster toaster, ILogger<EtherClient> logger)
        {
            // WebAssemblyHttpMessageHandler.DefaultCredentials = FetchCredentialsOption.Include;
            _httpClient = httpClient;
            _navigation = navigation;
            _toaster = toaster;
            _logger = logger;
            httpClient.BaseAddress = GetApiUrl();
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
                _logger.LogError(ex, "Cannot fetch user menu metadata");
                return false;
            }
        }

        public Task<AppHealthStatus> GetHealthStatus()
        {
            return HttpGet<AppHealthStatus>("health", checkStatusCode: false);
        }

        public Task<IEnumerable<T>> GetAll<T>()
        {
            return HttpGet<IEnumerable<T>>($"{GetPathFor<T>()}/GetAll");
        }

        public async Task<PageViewModel<T>> GetAllPaged<T>(int page = 1, int itemsPerPage = 10, Func<JObject, T> converter = null)
        {
            if (converter == null)
            {
                return await HttpGet<PageViewModel<T>>($"{GetPathFor<T>()}/GetAll?page={page}&itemsPerPage={itemsPerPage}");
            }

            var pageModel = await HttpGet<PageViewModel<JObject>>($"{GetPathFor<T>()}/GetAll?page={page}&itemsPerPage={itemsPerPage}");
            var items = pageModel.Items.Select(i => converter(i)).ToArray();
            return new PageViewModel<T>
            {
                Items = items,
                CurrentPage = pageModel.CurrentPage,
                TotalPages = pageModel.TotalPages
            };
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

        public Task<T> GetJobDetailsById<T>(Guid id)
        {
            return HttpGet<T>($"{GetPathFor<T>()}/GetJobDetails?id={id}");
        }

        public Task RunWorkitemsJob(IEnumerable<Guid> members, bool isReset)
        {
            return HttpPost("jobs/vsts/runworkitemsjob", new { Members = members, IsReset = isReset });
        }

        public Task RunPullRequestsJob(IEnumerable<Guid> members, bool isReset)
        {
            return HttpPost("jobs/vsts/runpullrequestsjob", new { Members = members, IsReset = isReset });
        }

        public Task<IEnumerable<ReporterDescriptorViewModel>> GetReportTypes()
        {
            return HttpGet<IEnumerable<ReporterDescriptorViewModel>>($"{GetPathFor<ReporterDescriptorViewModel>()}/types");
        }

        public async Task<byte[]> GenerateExcel(Guid id)
        {
            var url = $"{GetApiUrl()}report/generateexcel?id={id}";
            return await HttpGet<byte[]>(url);
        }

        public async Task<byte[]> DownloadGenerator()
        {
            var url = $"/Ether.EmailGenerator.zip";
            return await HttpGet<byte[]>(url);
        }

        public async Task<byte[]> GenerateEmail(GenerateEmailViewModel model)
        {
            var url = $"{GetApiUrl()}report/generateemail";
            return await HttpPost<byte[]>(url, model);
        }

        public async Task<ActiveWorkitemsViewModel> GetActiveWorkitems(Guid profileId)
        {
            var url = $"{GetApiUrl()}dashboard/activeworkitems?profileId={profileId}";
            return await HttpGet<ActiveWorkitemsViewModel>(url);
        }

        public async Task<AccessToken> RequestAccessToken(LoginViewModel model)
        {
            var tokenResponse = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = $"{GetApiUrl()}connect/token",
                ClientId = "EtherBlazorClient",
                UserName = model.UserName,
                Password = model.Password,
                Scope = "api"
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.ErrorDescription);
            }

            SetAccessToken(tokenResponse.AccessToken);
            return new AccessToken(tokenResponse.AccessToken, TimeSpan.FromSeconds(tokenResponse.ExpiresIn));
        }

        private async Task<T> HttpGet<T>(string url, bool checkStatusCode = true, [CallerMemberName] string caller = "")
        {
            var response = await _httpClient.GetAsync(url);
            if (checkStatusCode)
            {
                VerifyResponseStatusCode(response, caller);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        private Task HttpPost(string url, object payload, [CallerMemberName] string caller = "")
        {
            return HttpPostInternal(url, payload, caller);
        }

        private async Task<T> HttpPost<T>(string url, object payload, [CallerMemberName] string caller = "")
        {
            var response = await HttpPostInternal(url, payload, caller);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> HttpPostInternal(string url, object payload, string caller)
        {
            var json = JsonConvert.SerializeObject(payload);
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            VerifyResponseStatusCode(response, caller);

            return response;
        }

        private async Task HttpDelete(string url, [CallerMemberName] string caller = "")
        {
            var response = await _httpClient.DeleteAsync(url);
            VerifyResponseStatusCode(response, caller);
        }

        private void VerifyResponseStatusCode(HttpResponseMessage response, string caller)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _navigation.NavigateTo(Routes.LoginExpiredSession);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var message = $"Calling {caller} resulted in {response.StatusCode} {response.ReasonPhrase}";
                _toaster.Add(message, MatToastType.Danger, "Server responded with error", MatIconNames.Error);
                throw new EtherApiException(response.StatusCode, message);
            }
        }

        private string GetPathFor<T>()
        {
            var key = typeof(T);
            return _typeRoutes.ContainsKey(typeof(T)) ? _typeRoutes[key] : string.Empty;
        }

        private Uri GetApiUrl()
        {
            var baseUrl = _navigation.BaseUri;
            var isDevelopment = baseUrl.StartsWith("https://localhost");

            return isDevelopment ? new Uri("https://localhost:44315/api/") : new Uri($"{baseUrl}api/");
        }
    }
}
