using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;

namespace Ether.Tests.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TestDataAttribute : NUnitAttribute, IApplyToTest
    {
        private readonly int _membersCount;
        private readonly int _repositoryCount;
        private readonly int _projectsCount;

        public TestDataAttribute(int membersCount = 1, int repositoryCount = 1, int projectsCount = 1)
        {
            _membersCount = membersCount;
            _repositoryCount = repositoryCount;
            _projectsCount = projectsCount;
        }

        public int RelatedWorkItemsPerMember { get; set; }

        public void ApplyToTest(Test test)
        {
            test.Properties.Add(TestData.MembersCountKey, _membersCount);
            test.Properties.Add(TestData.RepositoryCountKey, _repositoryCount);
            test.Properties.Add(TestData.ProjectsCountKey, _projectsCount);

            if (RelatedWorkItemsPerMember != 0)
            {
                test.Properties.Add(TestData.RelatedWorkItemsCountKey, RelatedWorkItemsPerMember);
            }
        }
    }
}
