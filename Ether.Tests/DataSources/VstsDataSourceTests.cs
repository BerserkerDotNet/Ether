using System.Threading.Tasks;
using Ether.Contracts.Dto;
using Ether.Tests.Handlers;
using Ether.Vsts.Dto;
using Ether.Vsts.Types;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.DataSources
{
    public class VstsDataSourceTests : BaseHandlerTest
    {
        private VstsDataSource _dataSource;

        [Test]
        public async Task ShouldReturnProfileById()
        {
            var expectedProfile = Builder<VstsProfile>.CreateNew().Build();
            SetupSingle(expectedProfile, p => p == expectedProfile.Id);

            var result = await _dataSource.GetProfile(expectedProfile.Id);

            result.Id.Should().Be(expectedProfile.Id);
        }

        [Test]
        public async Task ShouldReturnTeamMemberById()
        {
            var expectedMember = Builder<TeamMember>.CreateNew().Build();
            SetupSingle(expectedMember, p => p == expectedMember.Id);

            var result = await _dataSource.GetTeamMember(expectedMember.Id);

            result.Id.Should().Be(expectedMember.Id);
        }

        [Test]
        public async Task ShouldReturnFilteredPullRequests()
        {
            var pullRequests = Builder<PullRequest>.CreateListOfSize(10)
                .All()
                .With(p => p.State = PullRequestState.Active)
                .TheFirst(2)
                .With(p => p.State = PullRequestState.Completed)
                .TheLast(2)
                .With(p => p.State = PullRequestState.Abandoned)
                .Build();
            SetupMultiple(pullRequests);

            var result = await _dataSource.GetPullRequests(p => p.State == ViewModels.Types.PullRequestState.Completed || p.State == ViewModels.Types.PullRequestState.Abandoned);

            result.Should().HaveCount(4);
            result.Should().OnlyContain(p => p.State == ViewModels.Types.PullRequestState.Completed || p.State == ViewModels.Types.PullRequestState.Abandoned);
        }

        protected override void Initialize()
        {
            _dataSource = new VstsDataSource(RepositoryMock.Object, Mapper);
        }
    }
}
