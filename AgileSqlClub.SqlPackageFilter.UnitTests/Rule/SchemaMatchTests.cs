using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileSqlClub.SqlPackageFilter.Filter;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Model;
using Moq;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.UnitTests
{
    [TestFixture]
    public class SchemaMatchTests
    {
        [Test]
        public void Does_Not_Match_Object_With_No_Schema_Name()
        {
            var rule = new SchemaFilterRule(FilterOperation.Ignore, "[a-zA-Z]");
            var objectName = new ObjectIdentifier("TableName");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Does_Match_Object_With_Schema_Name_That_Does_Match_Regex()
        {
            var rule = new SchemaFilterRule(FilterOperation.Ignore, "[a-zA-Z][a-zA-Z][a-zA-Z]");
            var objectName = new ObjectIdentifier("Sch","Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsTrue(result);
        }

        [Test]
        public void Does_Not_Match_Object_With_Schema_Name_That_Does_Not_Match_Regex()
        {
            var rule = new SchemaFilterRule(FilterOperation.Ignore, "[a-zA-Z]*[0-9]");
            var objectName = new ObjectIdentifier("Sch", "Tab");

            var result = rule.Matches(objectName, null);

            Assert.IsFalse(result);
        }

        [Test]
        public void Matches_Schema_Name_Using_ObjectType_When_Only_One_Part_Name_Specified()
        {
            var rule = new SchemaFilterRule(FilterOperation.Ignore, "Sch");
            var objectName = new ObjectIdentifier("Sch");
            var result = rule.Matches(objectName, ModelSchema.Schema);

            Assert.IsTrue(result);
        }
    }
}