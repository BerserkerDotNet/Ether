using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Contracts.Interfaces.CQS;
using Ether.Core.Types.Handlers.Queries;
using Ether.ViewModels;
using Ether.Vsts.Handlers.Queries;
using FluentAssertions;
using NUnit.Framework;

namespace Ether.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetAllHandlerTests : BaseHandlerTest
    {
        private GetAllFakeHandler _handler;

        [Test]
        public void ShouldNotThrowExceptionIfQueryIsNull()
        {
            SetupMultiple(Enumerable.Empty<Fake>());

            _handler.Awaiting(h => h.Handle(null))
                .Should().NotThrow();
        }

        [Test]
        public async Task ShouldReturnAllQueries()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(i => new Fake { Id = Guid.NewGuid() })
                .ToArray();

            SetupMultiple(expected);

            var result = await _handler.Handle(new GetAllFake());

            Mapper.Map<IEnumerable<Fake>>(result).Should().BeEquivalentTo(expected);
            RepositoryMock.VerifyAll();
        }

        protected override void Initialize()
        {
            _handler = new GetAllFakeHandler(RepositoryMock.Object, Mapper);
        }

        protected override void InitializeMappings(IMapperConfigurationExpression config)
        {
            config.CreateMap<Fake, FakeViewModel>();
            config.CreateMap<FakeViewModel, Fake>();
            base.InitializeMappings(config);
        }

        private class Fake : BaseDto
        {
        }

        private class FakeViewModel : ViewModelWithId
        {
        }

        private class GetAllFake : IQuery<IEnumerable<FakeViewModel>>
        {
        }

        private class GetAllFakeHandler : GetAllHandler<Fake, FakeViewModel, GetAllFake>
        {
            public GetAllFakeHandler(IRepository repository, IMapper mapper)
                : base(repository, mapper)
            {
            }
        }
    }
}
