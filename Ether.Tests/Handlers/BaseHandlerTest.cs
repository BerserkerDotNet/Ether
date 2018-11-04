using System;
using AutoMapper;
using Ether.Contracts.Dto;
using Ether.Contracts.Interfaces;
using Ether.Core.Config;
using ExpectedObjects;
using Moq;
using NUnit.Framework;

namespace Ether.Tests.Handlers
{
    public abstract class BaseHandlerTest
    {
        protected IMapper Mapper { get; private set; }

        protected Mock<IRepository> RepositoryMock { get; private set; }

        [SetUp]
        public void SetUp()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new CoreMappingProfile());
            });
            Mapper = mappingConfig.CreateMapper();
            RepositoryMock = new Mock<IRepository>(MockBehavior.Strict);

            Initialize();
        }

        protected void SetupNull<T>(Func<Guid, bool> predicate = null)
            where T : BaseDto
        {
            SetupSingle<T>(null);
        }

        protected void SetupSingle<T>(T value, Func<Guid, bool> predicate = null)
            where T : BaseDto
        {
            if (predicate == null)
            {
                predicate = _ => true;
            }

            RepositoryMock.Setup(r => r.GetSingleAsync<T>(It.Is<Guid>(e => predicate(e))))
                .ReturnsAsync(value)
                .Verifiable();
        }

        protected void SetupCreateOrUpdate<T, TModel>(TModel model)
            where T : BaseDto
        {
            var value = Mapper.Map<T>(model).ToExpectedObject();
            RepositoryMock.Setup(r => r.CreateOrUpdateAsync(It.Is<T>(v => value.Equals(v))))
                .ReturnsAsync(true)
                .Verifiable();
        }

        protected void SetupDelete<T>(Func<Guid, bool> predicate = null)
            where T : BaseDto
        {
            if (predicate == null)
            {
                predicate = _ => true;
            }

            RepositoryMock.Setup(r => r.DeleteAsync<T>(It.Is<Guid>(e => predicate(e))))
                .ReturnsAsync(true)
                .Verifiable();
        }

        protected virtual void Initialize()
        {
        }
    }
}
