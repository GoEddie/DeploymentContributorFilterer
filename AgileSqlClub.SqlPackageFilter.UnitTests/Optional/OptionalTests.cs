using AgileSqlClub.SqlPackageFilter.Extensions;
using NUnit.Framework;
using System;
using System.Linq;

namespace AgileSqlClub.SqlPackageFilter.UnitTests.Optional
{
    [TestFixture]
    public class OptionalTests
    {
        class MyObject2 : MyObject
        {
            public string MyValue { get; set; }
            public int MyNumber { get; set; }
        }

        class MyObject
        {
            public MyObject[] MyObjects { get; set; }
        }

        [Test]
        public void Optional_Nested_Value_In_Array_Should_Be_My_Value()
        {
            var myobject = new MyObject2
            {
                MyValue = "my value"
            };

            var myObjects = new MyObject
            {
                MyObjects = new MyObject[] { myobject }
            };

            var valueFromTest = String.Empty;
            var value =
                new Optional<MyObject>(myObjects)
                .ValueOrDefault(o => o?.MyObjects?.ToList())
                .OptionalAt(0).ValueOrDefault(o => o as MyObject2)
                .ValueOrDefault(o => o?.MyValue)
                .Default(() => "empty")
                .Evaluate();

            Assert.AreEqual("my value", value);
        }

        [Test]
        public void Optional_Nested_Value_In_Array_Should_Be_Empty()
        {
            var myobject = new MyObject2
            {
                MyValue = "my value"
            };

            var myObjects = new MyObject
            {
                MyObjects = new MyObject[] { } //empty array
            };

            var value =
                new Optional<MyObject>(myObjects)
                .ValueOrDefault(o => o?.MyObjects?.ToList())
                .OptionalAt(0).ValueOrDefault(o => o as MyObject2)
                .ValueOrDefault(o => o?.MyValue)
                .Default(() => "empty")
                .Evaluate();

            Assert.AreEqual("empty", value);
        }

        [Test]
        public void Optional_Empty_Object_Should_Be_Empty()
        {
            MyObject myObjects = null;

            var value =
                new Optional<MyObject>(myObjects)
                .ValueOrDefault(o => o?.MyObjects?.ToList())
                .OptionalAt(0).ValueOrDefault(o => o as MyObject2)
                .ValueOrDefault(o => o?.MyValue)
                .Default(() => "empty")
                .Evaluate();

            Assert.AreEqual("empty", value);
        }

        [Test]
        public void Optional_Empty_Object_Should_Be_Null()
        {
            MyObject myObjects = null;

            var value =
                new Optional<MyObject>(myObjects)
                .ValueOrDefault(o => o?.MyObjects?.ToList())
                .OptionalAt(0).ValueOrDefault(o => o as MyObject2)
                .ValueOrDefault(o => o?.MyValue)
                .Evaluate();

            Assert.AreEqual(null, value);
        }
    }
}
