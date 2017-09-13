using Ether.Core.Interfaces;
using Ether.Core.Configuration;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ether.Core.Models;

namespace Ether.Core.Reporters
{
    public abstract class ReporterBase : IReporter
    {
        protected readonly IRepository _repository;
        protected readonly ILogger<IReporter> _logger;
        protected readonly VSTSConfiguration _configuration;

        public ReporterBase(IRepository repository, IOptions<VSTSConfiguration> configuration, ILogger<IReporter> logger)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration.Value;
        }

        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public abstract Type ReportType { get; }

        public async Task<ReportResult> ReportAsync(ReportQuery query)
        {
            if (string.IsNullOrEmpty(_configuration.AccessToken) || string.IsNullOrEmpty(_configuration.InstanceName))
            {
                _logger.LogWarning("Attempt to generate report without proper configuration!");
                throw new ArgumentException("Configuration is missing.");
            }

            Input = await GetInputData(query);
            _logger.LogWarning("Report requested for {Profile} starting from {StartDate} until {EndDate}", Input.Profile.Name, query.StartDate, Input.ActualEndDate);

            var result = await ReportInternal();
            PopulateStandardFields(result);
            await _repository.CreateAsync(result);
            return result;
        }

        protected abstract Task<ReportResult> ReportInternal();

        private void PopulateStandardFields(ReportResult report)
        {
            report.Id = Guid.NewGuid();
            report.DateTaken = DateTime.UtcNow;
            report.StartDate = Input.Query.StartDate;
            report.EndDate = Input.Query.EndDate;
            report.ProfileName = Input.Profile.Name;
            report.ReporterId = Id;
            report.ReportName = Name;
        }

        private async Task<ReportInput> GetInputData(ReportQuery query)
        {
            var profile = await _repository.GetSingleAsync<Profile>(p => p.Id == query.ProfileId);
            if (profile == null)
                throw new ArgumentException("Selected profile was not found.");

            var repositories = await _repository.GetAsync<VSTSRepository>(r => profile.Repositories.Contains(r.Id));
            var members = await _repository.GetAsync<TeamMember>(m => profile.Members.Contains(m.Id));
            var projectsIds = repositories.Select(r => r.Project);
            var projects = await _repository.GetAsync<VSTSProject>(p => projectsIds.Contains(p.Id));

            return new ReportInput
            {
                Query = query,
                Profile = profile,
                Repositories = repositories,
                Members = members,
                Projects = projects
            };
        }

        protected ReportInput Input { get; private set; }

        protected class ReportInput
        {
            public ReportQuery Query { get; set; }
            public Profile Profile { get; set; }
            public IEnumerable<VSTSRepository> Repositories { get; set; }
            public IEnumerable<TeamMember> Members { get; set; }
            public IEnumerable<VSTSProject> Projects { get; set; }

            public VSTSProject GetProjectFor(VSTSRepository repo)
            {
                return Projects.SingleOrDefault(p => p.Id == repo.Project);
            }
            public DateTime ActualEndDate
            {
                get
                {
                    var today = DateTime.UtcNow.Date;
                    var endDate = today < Query.EndDate ? today : Query.EndDate;
                    return endDate.AddDays(1).AddMilliseconds(-1);
                }
            }
        }
    }
}
