using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ether.Types.Data
{
    public class VSTSApiUrl
    {
        private const string APIsSection = "_apis";
        private const string WITSection = "wit";
        private const string WIQLApiSection = "wiql";
        private const string WorkItemsSection = "WorkItems";
        private const string GITSection = "git";
        private const string RepositoriesSection = "repositories";
        private const string PullRequestsSection = "pullRequests";
        private const string APIVersion = "api-version";

        private StringBuilder _url = new StringBuilder();
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private VSTSApiUrl(string instance)
        {
            _url.Append($"https://{instance}.visualstudio.com");
        }

        public static VSTSApiUrl Create(string instance)
        {
            return new VSTSApiUrl(instance);
        }

        public VSTSApiUrl ForWIQL(string project)
        {
            _url.Append($"/{project}/{APIsSection}/{WITSection}/{WIQLApiSection}");
            return this;
        }

        public VSTSApiUrl ForWorkItems(int workItemId)
        {
            _url.Append($"/{APIsSection}/{WITSection}/{WorkItemsSection}/{workItemId}");
            return this;
        }

        public VSTSApiUrl ForRepository(string projectName, string repositoryName)
        {
            _url.Append($"/{projectName}/{APIsSection}/{GITSection}/{RepositoriesSection}/{repositoryName}");
            return this;
        }

        public VSTSApiUrl ForPullRequests(string projectName, string repositoryName)
        {
            ForRepository(projectName, repositoryName);
            _url.Append("/pullRequests");
            return this;
        }

        public VSTSApiUrl WithSection(string name)
        {
            _url.Append($"/{name}");
            return this;
        }

        public VSTSApiUrl WithQueryParameter(string name, string value)
        {
            _parameters.Add(name, value);
            return this;
        }

        public VSTSApiUrl Top(int count)
        {
            _parameters.Add("$top", count.ToString());
            return this;
        }

        public string Build()
        {
            _parameters.Add(APIVersion, "3.0");
            var queryString = "?" + string.Join("&", _parameters.Select(p => $"{p.Key}={p.Value}"));
            _url.Append(queryString);
            return _url.ToString();
        }
    }
}
