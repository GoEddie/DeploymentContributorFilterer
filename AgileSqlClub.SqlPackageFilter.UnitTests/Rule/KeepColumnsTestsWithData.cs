using System.Collections.Generic;
using AgileSqlClub.SqlPackageFilter.Filter;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Model;
using Moq;
using NUnit.Framework;
using Microsoft.SqlServer.Dac.Deployment;

namespace AgileSqlClub.SqlPackageFilter.UnitTests
{
    [TestFixture]
    public class StringSpliiterTests
    {

        [TestCase("this,that", "this", "that")]
        [TestCase("this-that", "this-that", null)]
        [TestCase(",that", "", "that")]
        public void SplitString(string input, string first,string last)
        {
            var result = input.SplitAtFirst(',');
            Assert.AreEqual(last == null ? 1 : 2, result.Length);

            Assert.AreEqual(first, result[0]);
            if (last != null)
                Assert.AreEqual(last, result[1]);

        }

    }
}
