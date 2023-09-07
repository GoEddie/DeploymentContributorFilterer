using AgileSqlClub.SqlPackageFilter.Filter;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Rule
{
    [TestFixture]
    public class MultiPartNamedObjectMatchTests
    {
        [Test]
        public void Matches_Name_That_Matches_Regex()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "[a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_Name_That_Matches_Regex_For_Right_To_Left()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "[a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("dbo", "Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }

        [Test]
        public void Does_Not_Match_Name_That_Does_Not_Match_Regex()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "^[0-9][a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Not_Match_Name_That_Does_Not_Match_Regex_For_Right_To_Left()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "^[0-9][a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("Tab", "WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Match_Name_That_Does_Not_Match_Regex_But_Is_A_Negative_Match_Type()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, ".*blah.*", MatchType.DoesNotMatch);
            var objectName = new ObjectIdentifier("WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_MultiPart_Name_That_Matches_Regex()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "dev,[a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("dev", "Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }
        [Test]
        public void Matches_MultiPart_Name_That_Does_Not_Match_Regex_But_Is_A_Negative_Match_Type()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "dev,[a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesNotMatch);
            var objectName = new ObjectIdentifier("dev", "Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Not_Match_MultiPart_Name_That_Does_Not_Match_Regex()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "dev,^[0-9][a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("dev", "WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Not_Match_MultiPart_Name_That_Does_Not_Match_Lhs_Regex()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "dbo,[0-9][a-zA-Z][a-zA-Z][a-zA-Z]", MatchType.DoesMatch);
            var objectName = new ObjectIdentifier("dev", "Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Match_MultiPart_Name_That_Does_Not_Match_Regex_But_Is_A_Negative_Match_Type()
        {
            var rule = new MultiPartNamedObjectFilterRule(FilterOperation.Ignore, "dev,.*blah.*", MatchType.DoesNotMatch);
            var objectName = new ObjectIdentifier("dev", "WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }
    }
}