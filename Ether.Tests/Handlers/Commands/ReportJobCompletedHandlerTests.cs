using System;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    public class ReportJobCompletedHandlerTests : BaseHandlerTest
    {
        private ReportJobStateHandler _handler;

        [Test]
        public void ShouldThrowExceptionIfJobTypeIsEmpty([Values(null, "")] string jobType)
        {
            _handler.Awaiting(h => h.Handle(ReportJobState.GetSuccessful(Guid.NewGuid(), jobType, DateTime.UtcNow, DateTime.UtcNow, null)))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ShouldCreateJobLogRecord()
        {
            const string jobType = "Foo";
            const string message = "Exception";
            RepositoryMock.Setup(r => r.CreateOrUpdateAsync(It.Is<JobLog>(l
                => l.JobType == jobType && l.Result == JobExecutionState.Failed && l.Error == message)))
                .ReturnsAsync(true)
                .Verifiable();

            await _handler.Handle(ReportJobState.GetFailed(Guid.NewGuid(), jobType, message, DateTime.UtcNow, DateTime.UtcNow));

            RepositoryMock.Verify();
        }

        protected override void Initialize()
        {
            _handler = new ReportJobStateHandler(RepositoryMock.Object, Mapper);
        }
    }
}
