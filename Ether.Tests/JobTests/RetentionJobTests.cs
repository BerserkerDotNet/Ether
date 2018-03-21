using Ether.Core.Interfaces;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ether.Tests.JobTests
{
    [TestFixture]
    public class RetentionJobTests
    {
        private Mock<IRepository> _repository;
        private Mock<ILogger<RetentionJob>> _logger;
        private RetentionJob _job;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IRepository>(MockBehavior.Strict);
            _logger = new Mock<ILogger<RetentionJob>>();
            _job = new RetentionJob(_repository.Object, _logger.Object);
        }

        [Test]
        public void ShouldExitIfNoSettings()
        {
            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult<Settings>(null));
            _job.Execute();
            _logger.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once());
            _repository.Verify(r => r.Delete(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()), Times.Never());
        }

        [Test]
        public void ShouldExitIfNoSpecificSettings()
        {
            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings()));
            _job.Execute();
            _repository.Verify(r => r.Delete(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()), Times.Never());
        }

        [Test]
        public void ShouldExitIfKeepLastIsNullSettings()
        {
            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings
            {
                WorkItemsSettings = new Settings.WorkItems { KeepLast = null },
                ReportsSettings = new Settings.Reports { KeepLast = null },
                PullRequestsSettings = new Settings.PullRequests { KeepLast = null }
            }));
            _job.Execute();
            _repository.Verify(r => r.Delete(It.IsAny<Expression<Func<VSTSWorkItem, bool>>>()), Times.Never());
        }

        [Test]
        public void ShouldCleanWorkitemsOlderThanKeepDate()
        {
            var workitems = new[]
            {
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow),
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow),
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(6))),
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(7))),
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(8))),
                Common.GetWorkItemWithDate(createdDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(9)))
            };
            var expectedToBeDeleted = workitems.TakeLast(3).Select(w => w.Id).ToList();

            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings
            {
                WorkItemsSettings = new Settings.WorkItems() { KeepLast = TimeSpan.FromDays(7) }
            }));
            _repository.Setup(r => r.GetAll<VSTSWorkItem>()).Returns(workitems);
            _repository.Setup(r => r.Delete(It.Is<Expression<Func<VSTSWorkItem, bool>>>(e => CheckWorkitemsExpression(e, workitems, expectedToBeDeleted))))
                .Returns<Expression<Func<VSTSWorkItem, bool>>>((e) => workitems.Count(e.Compile()));

            _job.Execute();

            _repository.Verify();
            _logger.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once());

        }

        [Test]
        public void ShouldLogErrorIfExceptionIsThrownUponCleanWorkitems()
        {
            var expectedException = new NotSupportedException();

            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings
            {
                WorkItemsSettings = new Settings.WorkItems() { KeepLast = TimeSpan.FromDays(7) }
            }));
            _repository.Setup(r => r.GetAll<VSTSWorkItem>()).Throws(expectedException);

            _job.Execute();

            _repository.Verify();
            _logger.Verify(l => l.Log(LogLevel.Error, 0, new FormattedLogValues("Error while cleaning workitems."), expectedException, It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        [Test]
        public void ShouldCleanReportsOlderThanKeepDate()
        {
            var reports = new[]
            {
               new ReportResult { DateTaken = DateTime.UtcNow },
               new ReportResult { DateTaken = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)) },
               new ReportResult { DateTaken = DateTime.UtcNow.Subtract(TimeSpan.FromDays(6)) },
               new ReportResult { DateTaken = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)) },
               new ReportResult { DateTaken = DateTime.UtcNow.Subtract(TimeSpan.FromDays(8)) },
               new ReportResult { DateTaken = DateTime.UtcNow.Subtract(TimeSpan.FromDays(9)) }
            };

            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings
            {
                ReportsSettings = new Settings.Reports() { KeepLast = TimeSpan.FromDays(7) }
            }));
            _repository.Setup(r => r.Delete(It.Is<Expression<Func<ReportResult, bool>>>(e => CheckReportsExpression(e, reports))))
                .Returns<Expression<Func<ReportResult, bool>>>(e => reports.Count(e.Compile()));

            _job.Execute();

            _repository.Verify();
            _logger.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        [Test]
        public void ShouldLogErrorIfExceptionIsThrownUponCleaningReports()
        {
            var expectedException = new NotSupportedException();
            _repository.Setup(r => r.GetSingleAsync<Settings>(_ => true)).Returns(Task.FromResult(new Settings
            {
                ReportsSettings = new Settings.Reports() { KeepLast = TimeSpan.FromDays(7) }
            }));

            _repository.Setup(r => r.Delete(It.IsAny<Expression<Func<ReportResult, bool>>>())).Throws(expectedException);

            _job.Execute();

            _repository.Verify();
            _logger.Verify(l => l.Log(LogLevel.Error, 0, new FormattedLogValues("Error while cleaning reports."), expectedException, It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        private bool CheckWorkitemsExpression(Expression<Func<VSTSWorkItem, bool>> e, VSTSWorkItem[] workitems, List<Guid> expectedToBeDeleted)
        {
            return workitems.Count(e.Compile()) == 3 && workitems.Where(e.Compile()).ToList().All(i => expectedToBeDeleted.Contains(i.Id));
        }

        private static bool CheckReportsExpression(Expression<Func<ReportResult, bool>> e, ReportResult[] reports)
        {
            var expectedToBeDeleted = reports.TakeLast(3).ToList();
            return reports.Count(e.Compile()) == 3 && reports.Where(e.Compile()).ToList().All(i => expectedToBeDeleted.Contains(i));
        }
    }
}
