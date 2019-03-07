using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class IgnoreTests
    {
        private readonly SqlGateway _gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);

        [Test]
        public void Schema_Is_Not_Dropped_When_Name_Is_Ignored()
        {
            
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bloobla') exec sp_executesql N'CREATE table blah.bloobla(id int)';");
            
            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(blah)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            var tableCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'blah';");

            Assert.AreEqual(1, tableCount, proc.Messages);

        }

        [Test]
        public void Schema_Is_Not_Created_When_Name_Is_Ignored()
        {
            
            
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')\r\nBEGIN DROP table dbo.Employees\r\nEND;");

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(dbo)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();

            var tableCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'Employees';");

            Assert.AreEqual(0, tableCount, proc.Messages);
        }

        [Test]
        public void Stored_Procedures_Are_Ignored_When_Objects_Ignored()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'proc_to_ignore')\r\nBEGIN exec sp_executesql N'create procedure proc_to_ignore as select 1;'\r\nEND;");

            var procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount);

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreType(.*Proced.*)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();

            procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount, proc.Messages);

        }

        [Test]
        public void Everything_Is_Ignored_Except_For_Schema_With_A_Negative_Filter()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'func') exec sp_executesql N'CREATE SCHEMA func';");
            _gateway.RunQuery("IF object_id('func.funky') is null exec sp_executesql N'CREATE FUNCTION func.funky() RETURNS INT AS  BEGIN  	RETURN 1;	 END';");

            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            _gateway.RunQuery("IF object_id('blah.funky_chicken') is null exec sp_executesql N'CREATE FUNCTION blah.funky_chicken() RETURNS INT AS  BEGIN  	RETURN 1;	 END';");


            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(1, count);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky_chicken';");
            Assert.AreEqual(1, count);
            
            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema!(func)\"";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky_chicken';");
            Assert.AreEqual(1, count, proc.Messages);
        }


        [Test]
        public void Negative_Filter_Excludes_Everything_Except_Specified_Schema_With_Additional_IgnoreSecurity_Also_Leaving_Security()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'func') exec sp_executesql N'CREATE SCHEMA func';");
            _gateway.RunQuery("IF object_id('func.funky') is null exec sp_executesql N'CREATE FUNCTION func.funky() RETURNS INT AS  BEGIN  	RETURN 1;	 END';");

            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            _gateway.RunQuery("IF object_id('blah.funky_chicken') is null exec sp_executesql N'CREATE FUNCTION blah.funky_chicken() RETURNS INT AS  BEGIN  	RETURN 1;	 END';");
            _gateway.RunQuery("IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name = 'fred')	CREATE USER fred WITHOUT LOGIN;");

            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(1, count);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky_chicken';");
            Assert.AreEqual(1, count);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.database_principals WHERE name = 'fred';");
            Assert.AreEqual(1, count);

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter1=IgnoreSchema!(func)\";\"SqlPackageFilter2=IgnoreSecurity\";";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.objects where name = 'funky_chicken';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.database_principals WHERE name = 'fred';");
            Assert.AreEqual(1, count);

        }


    }
}
