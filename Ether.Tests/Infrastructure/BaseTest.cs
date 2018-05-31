using System.Reflection;
using Ether.Tests.Infrastructure;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static NUnit.Framework.TestContext;

namespace Ether.Tests.Infrastructure
{
    [TestFixture]
    public abstract class BaseTest<T>
        where T : TestData, new()
    {
        [SetUp]
        public virtual void SetUp()
        {
            T data = new T();
            var testAdapter = TestContext.CurrentContext.Test;
            var membersCount = testAdapter.GetInt(TestData.MembersCountKey);
            var repositoryCount = testAdapter.GetInt(TestData.RepositoryCountKey);
            var projectsCount = testAdapter.GetInt(TestData.ProjectsCountKey);
            var relatedWorkItemsCount = testAdapter.GetInt(TestData.RelatedWorkItemsCountKey);
            data.WithBasicData(membersCount, repositoryCount, projectsCount)
                .WithConfiguration()
                .WithRepositoryMocks()
                .WithRelatedWorkItems(relatedWorkItemsCount);

            ProcessData(data);

            Data = data;
        }

        protected virtual void ProcessData(T data)
        {
        }

        protected T Data { get; set; }
    }


    [TestFixture]
    public class BaseTest : BaseTest<TestData>
    {
    }
}
