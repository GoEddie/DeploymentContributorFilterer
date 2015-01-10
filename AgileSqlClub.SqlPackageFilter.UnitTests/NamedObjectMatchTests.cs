using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.UnitTests
{
    [TestFixture]
    public class NamedObjectMatchTests
    {
        [Test]
        public void Matches_Name_That_Matches_Regex()
        {
            var rule = new NamedObjectFilterRule(FilterOperation.Ignore, "[a-zA-Z][a-zA-Z][a-zA-Z]");
            var objectName = new ObjectIdentifier("Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }

        [Test]
        public void Does_Not_Match_Name_That_Does_Not_Match_Regex()
        {
            var rule = new NamedObjectFilterRule(FilterOperation.Ignore, "^[0-9][a-zA-Z][a-zA-Z][a-zA-Z]");
            var objectName = new ObjectIdentifier("WOWSES9898989Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }
    }

    [TestFixture]
    public class IgnoreObjectTypeTests
    {
        [Test]
        public void Matches_Object_Type_That_Matches_Regex()
        {
            var rule = new ObjectTypeFilterRule(FilterOperation.Ignore, ".*");
            var type = ModelSchema.Aggregate;

            var result = rule.Matches(null, type);

            Assert.IsTrue(result);
        }

        [Test]
        public void Does_Not_Match_Object_Type_That_Dont_Match_Regex()
        {
            var rule = new ObjectTypeFilterRule(FilterOperation.Ignore, ".*ASDFGH.*");
            var type = ModelSchema.Aggregate;

            var result = rule.Matches(null, type);

            Assert.IsFalse(result);
        }
    }
}