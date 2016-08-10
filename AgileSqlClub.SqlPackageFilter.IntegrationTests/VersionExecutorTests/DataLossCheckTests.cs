using System.Configuration;
using NUnit.Framework;
using System.IO;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests.VersionExecutorTests
{
    [TestFixture]
    public class DacpacVersionTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Schema_Is_Not_Dropped_When_Name_Is_Ignored()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bloobla') exec sp_executesql N'CREATE table blah.bloobla(id int)';");

            var args =
            $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")}  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            var tableCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'blah';");

            Assert.AreEqual(1, tableCount, proc.Messages);

        }

    }
}
