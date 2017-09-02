using Ether.Interfaces;
using Ether.Types.Configuration;
using Ether.Types.DTO;
using Ether.Types.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Types.Reporters
{
    public abstract class ReporterBase : IReporter
    {
        protected readonly IRepository _repository;
        protected readonly ILogger<PullRequestsReporter> _logger;
        protected readonly VSTSConfiguration _configuration;

        public ReporterBase(IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<PullRequestsReporter> logger)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration.Value;
        }

        public abstract string Name { get; }

        public async Task<ReportResult> ReportAsync(ReportQuery query)
        {
            if (string.IsNullOrEmpty(_configuration.AccessToken) || string.IsNullOrEmpty(_configuration.InstanceName))
                throw new ArgumentException("Configuration is missing.");

            var input = await GetInputData(query);
            _logger.LogWarning("Report requested for {Profile} starting from {StartDate} until {EndDate}", input.Profile.Name, query.StartDate, query.EndDate);

            var result = await ReportInternal(input);
            PopulateStandartFields(result, input);
            await _repository.CreateAsync(result, typeOverride: typeof(ReportResult));
            return result;
        }

        protected abstract Task<ReportResult> ReportInternal(ReportInput input);

        private void PopulateStandartFields(ReportResult report, ReportInput input)
        {
            report.Id = Guid.NewGuid();
            report.DateTaken = DateTime.UtcNow;
            report.StartDate = input.Query.StartDate;
            report.EndDate = input.Query.EndDate;
            report.ProfileName = input.Profile.Name;
            report.ReportType = report.GetType().AssemblyQualifiedName;
            report.ReportName = Name;
        }

        private async Task<ReportInput> GetInputData(ReportQuery query)
        {
            var profile = await _repository.GetSingleAsync<Profile>(p => p.Id == query.ProfileId);
            if (profile == null)
                throw new ArgumentException("Selected profile was not found.");

            var repositories = await _repository.GetAsync<VSTSRepository>(r => profile.Repositories.Contains(r.Id));
            var members = await _repository.GetAsync<TeamMember>(m => profile.Members.Contains(m.Id));

            return new ReportInput
            {
                Query = query,
                Profile = profile,
                Repositories = repositories,
                Members = members
            };
        }

        protected class ReportInput
        {
            public ReportQuery Query { get; set; }
            public Profile Profile { get; set; }
            public IEnumerable<VSTSRepository> Repositories { get; set; }
            public IEnumerable<TeamMember> Members { get; set; }
        }
    }
}
