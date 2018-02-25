using Ether.Core.Configuration;
using Ether.Core.Interfaces;
using Ether.Core.Models;
using Ether.Core.Models.DTO;
using Ether.Core.Models.DTO.Reports;
using Ether.Core.Models.VSTS;
using Ether.Core.Models.VSTS.Response;
using Ether.Core.Reporters;
using Ether.Core.Types;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ether.Tests.Reporters
{
    [TestFixture]
    public class ListOfReviewersReporterTests
    {
        private Mock<IVstsClientRepository> _vstsRepositoryMock;
        private Mock<ILogger<ListOfReviewersReporter>> _loggerMock;
        private ListOfReviewersReporter _reporter;
        private Profile _profile;
        private TeamMember[] _team;

        [SetUp]
        public void SetUp()
        {
            _vstsRepositoryMock = new Mock<IVstsClientRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ListOfReviewersReporter>>();
            var repositoryMock = new Mock<IRepository>();
            var configMock = new Mock<IOptions<VSTSConfiguration>>();
            Common.SetupConfiguration(configMock);
            var data = Common.SetupDataForBaseReporter(repositoryMock, membersCount: 15, takeMembers: 10, repoCount: 1);
            _profile = data.profile;
            _team = data.members;
            _reporter = new ListOfReviewersReporter(_vstsRepositoryMock.Object, repositoryMock.Object, Mock.Of<IProgressReporter>(), configMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ShouldReturnEmptyReportIfNoData()
        {
            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(Enumerable.Empty<PullRequest>()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().BeEmpty();
            result.As<ListOfReviewersReport>()
                .NumberOfPullRequests.Should().Be(0);
            result.As<ListOfReviewersReport>()
                .NumberOfReviewers.Should().Be(0);

            _vstsRepositoryMock.Verify();
        }

        [Test]
        public async Task ShouldReturnUniqueListOfReviewersFromPullrequests()
        {
            const int expectedPRsCount = 100;
            var listOfPRs = GeneratePullRequests(expectedPRsCount, _team.Take(2));

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(listOfPRs))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(2);
            result.As<ListOfReviewersReport>().
                IndividualReports.All(c => _team[0].Email == c.UniqueName || _team[1].Email == c.UniqueName)
                .Should().BeTrue();
            result.As<ListOfReviewersReport>()
                .NumberOfPullRequests.Should().Be(expectedPRsCount);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldExtractReviewersFromComments()
        {
            var authors = new[]
            {
                new PullRequestReviewer { IsContainer = false, Vote = 5 , UniqueName = _team[0].Email},
                new PullRequestReviewer { IsContainer = false, Vote = 0 , UniqueName = _team[1].Email}
            };

            var comments = new[]
{
                new PullRequestThread.Comment { Author = authors[0] },
                new PullRequestThread.Comment { Author = authors[1] }
            };

            var prsList = new[]
            {
                new PullRequest { Reviewers = new [] { authors[0] }, Threads = new []{ new PullRequestThread { Comments = comments } } }
            };

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(prsList.AsEnumerable()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(2);
            result.As<ListOfReviewersReport>()
                .NumberOfPullRequests.Should().Be(1);
            result.As<ListOfReviewersReport>()
                .NumberOfReviewers.Should().Be(2);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldReturnTheCorrectNumberOfVotesInReport()
        {
            const int expectedPRsCount = 100;
            var listOfPRs = GeneratePullRequests(expectedPRsCount, _team.Take(2));

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(listOfPRs))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue
            });

            var expectedVotesForMember1 = listOfPRs.SelectMany(p => p.Reviewers)
                .Count(r => r.UniqueName == _team[0].Email && r.Vote != 0);
            var expectedVotesForMember2 = listOfPRs.SelectMany(p => p.Reviewers)
                .Count(r => r.UniqueName == _team[1].Email && r.Vote != 0);

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(2);
            result.As<ListOfReviewersReport>()
                .IndividualReports.Single(r => r.UniqueName == _team[0].Email)
                .NumberOfPRsVoted.Should().Be(expectedVotesForMember1);
            result.As<ListOfReviewersReport>()
                .IndividualReports.Single(r => r.UniqueName == _team[1].Email)
                .NumberOfPRsVoted.Should().Be(expectedVotesForMember2);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldReturnTheCorrectNumberOfCommentsInReport()
        {
            var authors = new[]
            {
                new PullRequestReviewer { IsContainer = false, Vote = 5 , UniqueName = _team[0].Email},
                new PullRequestReviewer { IsContainer = false, Vote = 0 , UniqueName = _team[1].Email}
            };

            var commentsSet1 = new[]
{
                new PullRequestThread.Comment { Author = authors[0] },
                new PullRequestThread.Comment { Author = authors[1] }
            };

            var commentsSet2 = new[]
            {
                new PullRequestThread.Comment { Author = authors[0] },
                new PullRequestThread.Comment { Author = authors[1] },
                new PullRequestThread.Comment { Author = authors[1] }
            };

            var threads = new[]
            {
                new PullRequestThread{ Comments = commentsSet1 },
                new PullRequestThread{ Comments = commentsSet2 }
            };

            var prsList = new[]
            {
                new PullRequest { Reviewers = new [] { authors[0] }, Threads = threads}
            };

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(prsList.AsEnumerable()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(2);
            result.As<ListOfReviewersReport>()
                .IndividualReports.Single(r => r.UniqueName == _team[0].Email)
                .NumberOfComments.Should().Be(2);
            result.As<ListOfReviewersReport>()
                .IndividualReports.Single(r => r.UniqueName == _team[1].Email)
                .NumberOfComments.Should().Be(3);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldIgnoreContainersAndNotVotedReviewers()
        {
            var prsList = new[]
            {
                new PullRequest
                {
                    Reviewers = new []
                    {
                        new PullRequestReviewer { IsContainer = true, Vote = 5 , UniqueName = "C1"},
                        new PullRequestReviewer { IsContainer = true, Vote = 0 , UniqueName = "C2"},
                        new PullRequestReviewer { IsContainer = true, Vote = -5 , UniqueName = "C3"},
                        new PullRequestReviewer { IsContainer = false, Vote = 0 , UniqueName = "R1"},
                    },
                    Threads = Enumerable.Empty<PullRequestThread>()
                }
            };

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(prsList.AsEnumerable()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().BeEmpty();
            result.As<ListOfReviewersReport>()
                .NumberOfPullRequests.Should().Be(1);
            result.As<ListOfReviewersReport>()
                .NumberOfReviewers.Should().Be(0);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldIgnoreSystemComments()
        {
            var authors = new[]
            {
                new PullRequestReviewer { IsContainer = false, Vote = 5 , UniqueName = _team[0].Email},
                new PullRequestReviewer { IsContainer = false, Vote = 5 , UniqueName = _team[1].Email},
            };

            var comments = new[]
            {
                new PullRequestThread.Comment { Author = authors[0] },
                new PullRequestThread.Comment { Author = authors[0], CommentType = "system" },
                new PullRequestThread.Comment { Author = authors[1], CommentType = "system" }
            };

            var prsList = new[]
            {
                new PullRequest { Reviewers = new [] { authors[0] }, Threads= new []{ new PullRequestThread{ Comments = comments } } }
            };

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(prsList.AsEnumerable()))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(1);
            result.As<ListOfReviewersReport>()
                .IndividualReports.Single(r => r.UniqueName == _team[0].Email)
                .NumberOfComments.Should().Be(1);

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldFilterByTeamMembers()
        {
            const int expectedPRsCount = 100;
            var theWholeTeam = _team.Take(2)
                .Union(new[] { new TeamMember() { Id = Guid.NewGuid(), Email = "outsider1@bar.com" }, new TeamMember() { Id = Guid.NewGuid(), Email = "outsider2@bar.com" } });
            var listOfPRs = GeneratePullRequests(expectedPRsCount, theWholeTeam);

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns(Task.FromResult(listOfPRs))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(2);
            result.As<ListOfReviewersReport>().
                IndividualReports.All(c => _team[0].Email == c.UniqueName || _team[1].Email == c.UniqueName)
                .Should().BeTrue();

            _vstsRepositoryMock.VerifyAll();
        }

        [Test]
        public async Task ShouldFilterByDate()
        {
            var utcNow = DateTime.UtcNow;
            var prsList = Enumerable.Range(0, 10).Select(i => new PullRequest
            {
                CreationDate = utcNow.AddDays(-i),
                Reviewers = new[] { new PullRequestReviewer { IsContainer = false, Vote = 5, UniqueName = _team[i].Email } },
                Threads = Enumerable.Empty<PullRequestThread>()
            });

            _vstsRepositoryMock.Setup(c => c.GetPullRequests(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PullRequestQuery>()))
                .Returns<string, string, PullRequestQuery>((p, r, q) => Task.FromResult(prsList.Where(pr => q.Filter(pr))))
                .Verifiable();

            var result = await _reporter.ReportAsync(new ReportQuery
            {
                ProfileId = _profile.Id,
                StartDate = utcNow.AddDays(-4),
                EndDate = utcNow
            });

            result.Should().NotBeNull();
            result.As<ListOfReviewersReport>()
                .IndividualReports.Should().HaveCount(5);
            result.As<ListOfReviewersReport>()
                .NumberOfPullRequests.Should().Be(5);

            _vstsRepositoryMock.VerifyAll();
        }

        private IEnumerable<PullRequest> GeneratePullRequests(int count, IEnumerable<TeamMember> reviewersPool)
        {
            var random = new Random();
            return Enumerable.Range(0, count).Select(i => new PullRequest
            {
                PullRequestId = i,
                CreationDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(i)),
                Reviewers = GenerateReviewers(reviewersPool).ToArray(),
                Threads = Enumerable.Empty<PullRequestThread>()
            }).ToList();
        }

        private IEnumerable<PullRequestReviewer> GenerateReviewers(IEnumerable<TeamMember> reviewersPool)
        {
            var random = new Random();
            var allowedVotes = new[] { -10, 0, 5, 10 };
            return Enumerable.Range(0, reviewersPool.Count()).Select(i => new PullRequestReviewer
            {
                Vote = allowedVotes[random.Next(0, allowedVotes.Count())],
                UniqueName = reviewersPool.ElementAt(i).Email
            }).Union(new[] 
            {
                new PullRequestReviewer { IsContainer = true, UniqueName = "C1", Vote = 5 },
                new PullRequestReviewer { IsContainer = true, UniqueName = "C2", Vote = 0 }
            });
        }
    }
}
