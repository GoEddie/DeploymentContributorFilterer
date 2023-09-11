using System.Configuration;
using NUnit.Framework;
using System.IO;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class KeepTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Can_1_Publish_reset_to_a_known_state()
        {

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")}" +
                $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true /p:BlockOnPossibleDataLoss=False ";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void Schema_Is_Not_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ohwahweewah') exec sp_executesql N'CREATE SCHEMA ohwahweewah';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " + 
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepName(ohwahweewah)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Everything_In_Schema_Is_Not_Dropped_When_Schema_Is_To_Keep()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'ohwahweewah') exec sp_executesql N'CREATE SCHEMA ohwahweewah';");
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BLAHHHkjkjk') exec sp_executesql N'CREATE TABLE ohwahweewah.BLAHHHkjkjk(id int)';");

            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables WHERE name = 'BLAHHHkjkjk';");
            Assert.AreEqual(1, count);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepSchema(ohwahweewah)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables WHERE name = 'BLAHHHkjkjk';");
            Assert.AreEqual(1, count, proc.Messages);

           
        }



        [Test]
        public void ObjectType_Is_Not_Dropped_When_Filter_Is_To_Keep()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'funky') exec sp_executesql N'CREATE FUNCTION funky() RETURNS INT AS  BEGIN  	RETURN 1;	 END';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(1, count);

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepType(ScalarFunction)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            
            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(1, count, proc.Messages);
        
        
        }
        
    }


}