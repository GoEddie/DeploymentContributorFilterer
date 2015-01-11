using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Filter;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Config
{
    /// <summary>
    /// Rule definitions that are in /p:AdditionalDeploymentContributorArguments
    /// 
    /// These rule definitions are like:
    /// 
    /// /p:AdditionalDeploymentContributorArguments="[Filter|FilterFile=[Ignore|Keep][Schema|Name|Type][(Regex)]"
    /// 
    /// </summary>
    [TestFixture]
    public class FilterArgsTests
    {
        [Test]
        public void Parses_Ignore_Operation()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("IgnoreSchema([a-zA-Z]99.*)");

            Assert.AreEqual(FilterOperation.Ignore, definition.Operation);
            
        }
        
        [Test]
        public void Parses_Keep_Operation()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("KeepType([a-zA-Z]99.*)");

            Assert.AreEqual(FilterOperation.Keep, definition.Operation);
        }





        [Test]
        public void Parses_Schema_Type()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("KeepSchema([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Schema, definition.Type);
        }

        [Test]
        public void Parses_Type_Type()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("KeepType([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Type, definition.Type);
        }

        [Test]
        public void Parses_Name_Type()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("KeepName([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Name, definition.Type);
        }

        [Test]
        public void Parses_Match()
        {
            var parser = new CommandLineFilterParser();
            var definition = parser.GetDefinitions("KeepName([a-zA-Z]99.*)");

            Assert.AreEqual("[a-zA-Z]99.*", definition.Match);
        }

        
    }
}
