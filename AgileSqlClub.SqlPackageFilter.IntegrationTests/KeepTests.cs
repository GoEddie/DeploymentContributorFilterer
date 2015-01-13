using System.Configuration;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class KeepTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Schema_Is_Not_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ohwahweewah') exec sp_executesql N'CREATE SCHEMA ohwahweewah';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);

            var args =
                "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " + 
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True" +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepName(ohwahweewah)\"";

            var proc = new ProcessGateway(".\\SqlPackage.exe\\SqlPackage.exe", args);
            proc.Run();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.GetMessages());

            
        }

    }
}