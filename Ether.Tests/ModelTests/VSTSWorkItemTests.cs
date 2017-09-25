using Ether.Core.Models.VSTS;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Ether.Tests.ModelTests
{
    [TestFixture]
    public class VSTSWorkItemTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("foo")]
        [TestCase("5/78")]
        public void ShouldReturnNullIfIncorrectInput(string value)
        {
            var wi = GetWithSameDates(value);

            wi.ChangedDate.HasValue.Should().BeFalse();
            wi.ResolvedDate.HasValue.Should().BeFalse();
            wi.ClosedDate.HasValue.Should().BeFalse();
            wi.StateChangeDate.HasValue.Should().BeFalse();
        }

        [TestCase("2017-9-18T20:49:20.77Z")]
        [TestCase("2016-2-29T20:49:20.77Z")]
        public void ShouldParseDateString(string value)
        {
            var expected = DateTime.Parse(value);

            var wi = GetWithSameDates(value);

            wi.ChangedDate.Value.Should().Be(expected);
            wi.ResolvedDate.Value.Should().Be(expected);
            wi.ClosedDate.Value.Should().Be(expected);
            wi.StateChangeDate.Value.Should().Be(expected);
        }

        [Test]
        public void ShouldReturnDateForSpecificField()
        {
            const string changedDate = "2017-9-18T12:49:23.77Z";
            const string resolvedDate = "2017-2-12T14:49:41.77Z";
            const string closedDate = "2017-5-03T12:49:11.77Z";
            const string stateChangeDate = "2017-6-25T11:49:56.77Z";

            var expectedChangedDate = DateTime.Parse(changedDate);
            var expectedResolvedDate = DateTime.Parse(resolvedDate);
            var expectedClosedDate = DateTime.Parse(closedDate);
            var expectedStateChangeDate = DateTime.Parse(stateChangeDate);

            var wi = GetWithDates(changedDate, resolvedDate, closedDate, stateChangeDate);

            wi.ChangedDate.Value.Should().Be(expectedChangedDate);
            wi.ResolvedDate.Value.Should().Be(expectedResolvedDate);
            wi.ClosedDate.Value.Should().Be(expectedClosedDate);
            wi.StateChangeDate.Value.Should().Be(expectedStateChangeDate);
        }

        [Test]
        public void ShouldReturnEmptyStringIfFieldIsNotInTheCollection()
        {
            var wi = new VSTSWorkItem { Fields = new Dictionary<string, string>() };
            wi.WorkItemType.Should().BeEmpty();
        }

        [Test]
        public void ShouldReturnEmptyStringIfFieldsAreNull()
        {
            var wi = new VSTSWorkItem { Fields = null };
            wi.Reason.Should().BeEmpty();
        }

        private VSTSWorkItem GetWithSameDates(string value)
        {
            return GetWithDates(value, value, value, value);
        }

        private VSTSWorkItem GetWithDates(string changedDate, string resolvedDate, string closedDate, string stateChangeDate)
        {
            var wi = new VSTSWorkItem();
            wi.Fields = new Dictionary<string, string>();
            wi.Fields.Add("System.ChangedDate", changedDate);
            wi.Fields.Add("Microsoft.VSTS.Common.ResolvedDate", resolvedDate);
            wi.Fields.Add("Microsoft.VSTS.Common.ClosedDate", closedDate);
            wi.Fields.Add("Microsoft.VSTS.Common.StateChangeDate", stateChangeDate);

            return wi;
        }
    }
}
