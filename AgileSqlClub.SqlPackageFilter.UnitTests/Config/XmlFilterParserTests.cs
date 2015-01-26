using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Filter;
using NUnit.Framework;
using Moq;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Config
{
    [TestFixture]
    class XmlFilterParserTests
    {
        [Test]
        public void BuildsRuleDefinition_For_Single_Rule_With_No_Match()
        {
            var xml = @"<DeploymentFilter><Filter Operation=""Ignore"" Type=""Security"" MatchType=""DoesMatch"" /></DeploymentFilter>";
            var gateway = new Mock<IFileGateway>();
            gateway.Setup(p => p.GetContents(It.IsAny<string>())).Returns(xml);

            var parser = new XmlFilterParser(gateway.Object);
            var defintions = parser.GetDefinitions("dsds");

            Assert.AreEqual(1, defintions.Count());
            Assert.AreEqual(FilterOperation.Ignore, defintions.FirstOrDefault().Operation);
            Assert.AreEqual(FilterType.Security, defintions.FirstOrDefault().FilterType);
            Assert.AreEqual(MatchType.DoesMatch, defintions.FirstOrDefault().MatchType);
            Assert.IsTrue(string.IsNullOrEmpty(defintions.FirstOrDefault().Match));
        }

        [Test]
        public void BuildsRuleDefinition_For_Single_Rule_With_Match_Criteria()
        {            
            var xml = @"<DeploymentFilter><Filter Operation=""Ignore"" Type=""Schema"" MatchType=""DoesNotMatch"">dbo</Filter></DeploymentFilter>";
            var gateway = new Mock<IFileGateway>();
            gateway.Setup(p => p.GetContents(It.IsAny<string>())).Returns(xml);

            var parser = new XmlFilterParser(gateway.Object);
            var defintions = parser.GetDefinitions("dsds");

            Assert.AreEqual(1, defintions.Count());
            Assert.AreEqual(FilterOperation.Ignore, defintions.FirstOrDefault().Operation);
            Assert.AreEqual(FilterType.Schema, defintions.FirstOrDefault().FilterType);
            Assert.AreEqual(MatchType.DoesNotMatch, defintions.FirstOrDefault().MatchType);
            Assert.AreEqual("dbo", defintions.FirstOrDefault().Match);

        }

        [Test]
        public void BuildsRuleDefinition_For_Negative_Rule()
        {
            //Do not deploy anything that is not in the dbo schema 
            var xml = @"<DeploymentFilter><Filter Operation=""Keep"" Type=""Schema"" MatchType=""DoesNotMatch"">dbo</Filter></DeploymentFilter>";
            var gateway = new Mock<IFileGateway>();
            gateway.Setup(p => p.GetContents(It.IsAny<string>())).Returns(xml);

            var parser = new XmlFilterParser(gateway.Object);
            var defintions = parser.GetDefinitions("dsds");

            Assert.AreEqual(1, defintions.Count());
            Assert.AreEqual(FilterOperation.Keep, defintions.FirstOrDefault().Operation);
            Assert.AreEqual(MatchType.DoesNotMatch, defintions.FirstOrDefault().MatchType);
            Assert.AreEqual(FilterType.Schema, defintions.FirstOrDefault().FilterType);
            Assert.AreEqual("dbo", defintions.FirstOrDefault().Match);

        }


        [Test]
        public void BuildsRuleDefinition_For_Multiple_Rules()
        {
            var xml = @"<DeploymentFilter><Filter Operation=""Ignore"" Type=""Schema"" MatchType=""DoesNotMatch"">dbo</Filter><Filter Operation=""Keep"" Type=""Name"" MatchType=""DoesMatch"">FishFace</Filter><Filter Operation=""Ignore"" Type=""Name"" MatchType=""DoesMatch"">DogPound</Filter></DeploymentFilter>";
            var gateway = new Mock<IFileGateway>();
            gateway.Setup(p => p.GetContents(It.IsAny<string>())).Returns(xml);

            var parser = new XmlFilterParser(gateway.Object);
            var defintions = parser.GetDefinitions("dsds");

            Assert.AreEqual(3, defintions.Count());
            Assert.AreEqual(FilterOperation.Ignore, defintions.FirstOrDefault().Operation);
            Assert.AreEqual(FilterType.Schema, defintions.FirstOrDefault().FilterType);
            Assert.AreEqual(MatchType.DoesNotMatch, defintions.FirstOrDefault().MatchType);
            Assert.AreEqual("dbo", defintions.FirstOrDefault().Match);

            var second = defintions.ElementAt(1);
            Assert.AreEqual("FishFace", second.Match);
            Assert.AreEqual(FilterOperation.Keep, second.Operation);

            var third = defintions.ElementAt(2);
            Assert.AreEqual("DogPound", third.Match);
            Assert.AreEqual(FilterOperation.Ignore, third.Operation);

        }


        [Test]
        public void BuildsRuleDefinition_Reports_Message_When_No_Rules_Found()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void BuildsRuleDefinition_Reports_Message_When_File_Corrupt()
        {
            throw new NotImplementedException();
        }



    }
}
