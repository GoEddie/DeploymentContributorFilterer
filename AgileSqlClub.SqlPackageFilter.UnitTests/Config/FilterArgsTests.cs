using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Filter;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Config
{
    /// <summary>
    /// Rule definitions that are in /p:AdditionalDeploymentContributorArguments
    /// 
    /// These rule definitions are like:
    /// 
    /// /p:AdditionalDeploymentContributorArguments="[Filter|FilterFile=[Ignore|Keep][Schema|Name|Type][(Regex)]"
    /// 
    /// 
    /// !(Regex) to match everything except the regex - useful when you want to say deploy a dac pac but don't want to include one schema or something.
    /// 
    /// </summary>
    [TestFixture]
    public class FilterArgsTests
    {
        readonly IDisplayMessageHandler _handler = new TestDisplayMessageHandler();

        [Test]
        public void Parses_Ignore_Operation()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("IgnoreSchema([a-zA-Z]99.*)");

            Assert.AreEqual(FilterOperation.Ignore, definition.Operation);
            
        }
        
        [Test]
        public void Parses_Keep_Operation()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepType([a-zA-Z]99.*)");

            Assert.AreEqual(FilterOperation.Keep, definition.Operation);
        }
        
        [Test]
        public void Parses_Schema_Type()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepSchema([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Schema, definition.FilterType);
        }

        [Test]
        public void Parses_Type_Type()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepType([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Type, definition.FilterType);
        }

        [Test]
        public void Parses_Name_Type()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepName([a-zA-Z]99.*)");

            Assert.AreEqual(FilterType.Name, definition.FilterType);
        }

        [Test]
        public void Parses_Match()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepName([a-zA-Z]99.*)");
            Assert.AreEqual(MatchType.DoesMatch, definition.MatchType);
            Assert.AreEqual("[a-zA-Z]99.*", definition.Match);
        }

        [Test]
        public void Parses_NonMatch()
        {
            var parser = new CommandLineFilterParser(_handler);
            var definition = parser.GetDefinitions("KeepName!([a-zA-Z]99.*)");

            Assert.AreEqual(MatchType.DoesNotMatch ,definition.MatchType);
            Assert.AreEqual("[a-zA-Z]99.*", definition.Match);
        }
        
    }
}
