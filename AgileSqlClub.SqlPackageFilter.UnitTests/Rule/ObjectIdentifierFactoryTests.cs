using AgileSqlClub.SqlPackageFilter.Filter;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.UnitTests
{
  [TestFixture]
  public class ObjectIdentifierFactoryTests
  {
    [Test]
    public void One_Part_Names()
    {
      var act1 = ObjectIdentifierFactory.Create("Object");
      var act2 = ObjectIdentifierFactory.Create("[Object]");

      Assert.AreEqual("[Object]", act1.ToString());
      Assert.AreEqual(act1.ToString(), act2.ToString());
      Assert.IsFalse(act1.HasExternalParts);
      Assert.IsFalse(act2.HasExternalParts);
      Assert.AreEqual(1, act1.Parts.Count);
      Assert.AreEqual(1, act2.Parts.Count);
    }

    [Test]
    public void Two_Part_Names()
    {
      var act1 = ObjectIdentifierFactory.Create("Schema.Object");
      var act2 = ObjectIdentifierFactory.Create("[Schema].[Object]");
      var act3 = ObjectIdentifierFactory.Create("Schema.[Object]");
      var act4 = ObjectIdentifierFactory.Create("[Schema].Object");

      Assert.AreEqual("[Schema].[Object]", act1.ToString());
      Assert.AreEqual(act1.ToString(), act2.ToString());
      Assert.AreEqual(act1.ToString(), act3.ToString());
      Assert.AreEqual(act1.ToString(), act4.ToString());
      Assert.IsFalse(act1.HasExternalParts);
      Assert.IsFalse(act2.HasExternalParts);
      Assert.IsFalse(act3.HasExternalParts);
      Assert.IsFalse(act4.HasExternalParts);
      Assert.AreEqual(2, act1.Parts.Count);
      Assert.AreEqual(2, act2.Parts.Count);
      Assert.AreEqual(2, act3.Parts.Count);
      Assert.AreEqual(2, act4.Parts.Count);
    }

    [Test]
    public void Three_Part_Names()
    {
      var act1 = ObjectIdentifierFactory.Create("Database.Schema.Object");
      var act2 = ObjectIdentifierFactory.Create("[Database].[Schema].[Object]");

      Assert.AreEqual("[Database].[Schema].[Object]", act1.ToString());
      Assert.AreEqual(act1.ToString(), act2.ToString());
      Assert.IsTrue(act1.HasExternalParts);
      Assert.IsTrue(act2.HasExternalParts);
      Assert.AreEqual(2, act1.Parts.Count);
      Assert.AreEqual(2, act2.Parts.Count);
      Assert.AreEqual(1, act1.ExternalParts.Count);
      Assert.AreEqual(1, act2.ExternalParts.Count);
    }

    [Test]
    public void Four_Part_Names()
    {
      var act1 = ObjectIdentifierFactory.Create("Server.Database.Schema.Object");
      var act2 = ObjectIdentifierFactory.Create("[Server].[Database].[Schema].[Object]");

      Assert.AreEqual("[Server].[Database].[Schema].[Object]", act1.ToString());
      Assert.AreEqual(act1.ToString(), act2.ToString());
      Assert.IsTrue(act1.HasExternalParts);
      Assert.IsTrue(act2.HasExternalParts);
      Assert.AreEqual(2, act1.Parts.Count);
      Assert.AreEqual(2, act2.Parts.Count);
      Assert.AreEqual(2, act1.ExternalParts.Count);
      Assert.AreEqual(2, act2.ExternalParts.Count);
    }
  }
}