using System;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Ether.Contracts.Dto;
using Ether.Contracts.Dto.Reports;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Commands;
using Ether.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ether.Core.Types.Handlers.Commands
{
    public abstract class GenerateReportHandlerBase<TCommand> : ICommandHandler<TCommand, Guid>
        where TCommand : GenerateReportCommand
    {
        private readonly IIndex<string, IDataSource> _dataSources;
        private readonly IRepository _repository;

        public GenerateReportHandlerBase(IIndex<string, IDataSource> dataSources, IRepository repository, ILogger logger)
        {
            _dataSources = dataSources;
            _repository = repository;
            Logger = logger;
        }

        protected ILogger Logger { get; }

        public async Task<Guid> Handle(TCommand command)
        {
            var dataSourceType = await _repository.GetFieldValueAsync<Profile, string>(p => p.Id == command.Profile, p => p.Type);
            if (!_dataSources.TryGetValue(dataSourceType, out var dataSource))
            {
                throw new ArgumentException($"Data source of type {dataSourceType} is not supported.");
            }

            var profile = await dataSource.GetProfile(command.Profile);
            if (profile == null)
            {
                throw new ArgumentException("Requested profile is not found.");
            }

            Logger.LogInformation("Starting to generate {DataSource} PullRequest report for {Profile}, range: {Start} {End}", dataSourceType, profile.Name, command.Start, command.End);
            var report = await GenerateAsync(command, dataSource, profile);
            Logger.LogInformation("Finished generating {DataSource} PullRequest report for {Profile}, range: {Start} {End}", dataSourceType, profile.Name, command.Start, command.End);
            var info = GetReportInfo();
            report.Id = Guid.NewGuid();
            report.DateTaken = DateTime.UtcNow;
            report.StartDate = command.Start;
            report.EndDate = command.End;
            report.ProfileName = profile.Name;
            report.ProfileId = profile.Id;
            report.ReportType = info.type;
            report.ReportName = info.name;

            await _repository.CreateAsync(report);
            return report.Id;
        }

        protected abstract Task<ReportResult> GenerateAsync(TCommand command, IDataSource dataSource, ProfileViewModel profile);

        protected abstract (string type, string name) GetReportInfo();
    }
}
