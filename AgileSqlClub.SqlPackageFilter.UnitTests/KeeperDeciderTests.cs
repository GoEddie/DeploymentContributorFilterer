using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Model;
using Moq;
using NUnit.Framework;
using AgileSqlClub.SqlPackageFilter;

namespace AgileSqlClub.SqlPackageFilter.UnitTests
{
    [TestFixture]
    public class KeeperDeciderTests
    {
        [Test]
        public void Should_Only_Try_Keep_Rules_When_It_Is_A_Drop()
        {
            var ignoreRule = new Mock<FilterRule>();
            ignoreRule.Setup(p=>p.Operation()).Returns(FilterOperation.Ignore);

            var keepRule = new Mock<FilterRule>();
            keepRule.Setup(p => p.Operation()).Returns(FilterOperation.Keep);

            ignoreRule.Setup(p => p.Matches(It.IsAny<TSqlObject>())).Returns(false);

            keepRule.Setup(p => p.Matches(It.IsAny<TSqlObject>()))
                .Throws(new Exception("KeepRule.Match should not have been called"));

            var decider = new KeeperDecider(new List<FilterRule>() {ignoreRule.Object, keepRule.Object});

            decider.ShouldRemoveFromPlan(null, false);

            keepRule.Setup(p => p.Matches(It.IsAny<TSqlObject>()));

            decider.ShouldRemoveFromPlan(null, true);

            keepRule.Verify(p=>p.Matches(It.IsAny<TSqlObject>()));
        }

    }
}
