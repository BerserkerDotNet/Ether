using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Contracts.Types;
using Ether.Core.Types.Commands;
using Ether.Core.Types.Handlers.Commands;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Commands
{
    [TestFixture]
    public class AbortActiveJobsHandlerTests : BaseHandlerTest
    {
        private AbortActiveJobsHandler _handler;

        [Test]
        public void ShouldSuppressExceptions()
        {
            RepositoryMock.Setup(r => r.UpdateFieldValue(
                It.IsAny<Expression<Func<JobLog, bool>>>(),
                It.IsAny<Expression<Func<JobLog, JobExecutionState>>>(),
                It.IsAny<JobExecutionState>()))
                .Throws<Exception>();

            _handler.Awaiting(h => h.Handle(new AbortActiveJobs()))
                .Should().NotThrow();
        }

        [Test]
        public void ShouldNotThrowIfCommandIsNull()
        {
            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldUpdateResultFieldWithCorrectValue()
        {
            RepositoryMock.Setup(r => r.UpdateFieldValue(
                It.Is<Expression<Func<JobLog, bool>>>(e => VerifyFilterExpression(e)),
                It.IsAny<Expression<Func<JobLog, JobExecutionState>>>(),
                JobExecutionState.Aborted))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _handler.Handle(new AbortActiveJobs());

            RepositoryMock.Verify();
        }

        protected override void Initialize()
        {
            _handler = new AbortActiveJobsHandler(RepositoryMock.Object, Mock.Of<ILogger<AbortActiveJobsHandler>>());
        }

        private bool VerifyFilterExpression(Expression<Func<JobLog, bool>> filter)
        {
            var body = filter.Body as BinaryExpression;
            var type = body.Right as ConstantExpression;
            return (int)type.Value == (int)JobExecutionState.InProgress;
        }
    }
}
