using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Configuration;
using System.IO;

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
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void Schema_Is_Not_Created_When_Name_Is_Ignored()
        {
            
            
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')\r\nBEGIN DROP table dbo.Employees\r\nEND;");

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(dbo)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            var tableCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'Employees';");

            Assert.AreEqual(0, tableCount, proc.Messages);
            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Stored_Procedures_Are_Ignored_When_Objects_Ignored()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'proc_to_ignore')\r\nBEGIN exec sp_executesql N'create procedure proc_to_ignore as select 1;'\r\nEND;");

            var procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount);

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreType(.*Proced.*)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();

            procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount, proc.Messages);
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void Stored_Procedures_Are_Ignored_When_Objects_Ignored_And_Schema_Matchs()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'proc_to_ignore' AND SCHEMA_NAME(schema_id) = 'dbo')\r\nBEGIN exec sp_executesql N'create procedure [dbo].[proc_to_ignore] as select 1;'\r\nEND;");

            var procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore' AND SCHEMA_NAME(schema_id) = 'dbo';");

            Assert.AreEqual(1, procCount);

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreType(.*Proced.*, dbo)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();

            procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore' AND SCHEMA_NAME(schema_id) = 'dbo';");

            Assert.AreEqual(1, procCount, proc.Messages);
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void Stored_Procedures_Are_Dropped_When_Objects_Ignored_And_Schema_Does_Not_Matchs()
        {
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'proc_to_drop' AND SCHEMA_NAME(schema_id) = 'dbo')\r\nBEGIN exec sp_executesql N'create procedure [dbo].[proc_to_drop] as select 1;'\r\nEND;");

            var procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_drop' AND SCHEMA_NAME(schema_id) = 'dbo';");

            Assert.AreEqual(1, procCount);

            var args =
            $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreType(.*Proced.*, guest)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();

            procCount = _gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_drop' AND SCHEMA_NAME(schema_id) = 'dbo';");

            Assert.AreEqual(0, procCount, proc.Messages);
            Assert.Pass(proc.Messages);

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
            Assert.Pass(proc.Messages);
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
            Assert.Pass(proc.Messages);

        }

        [Test]
        public void IgnoreSchema_Filter_Applies_To_ColumnStoreIndex()
        {
            //dacpac defines a table with a columnstore index.  this test will ignore any changes in the schema this table is in.
            
            //ensure this table does not exist, so we can check the dacpac did not create it
            _gateway.RunQuery("exec sp_executesql N'DROP TABLE IF EXISTS dbo.TableName';");
            _gateway.RunQuery("exec sp_executesql N'DROP TABLE IF EXISTS dbo.Employees';");

            //create dummy table with unnamed default constraint to ensure it doesn't get dropped
            _gateway.RunQuery("exec sp_executesql N'DROP TABLE IF EXISTS dbo.Dummy';");
            _gateway.RunQuery("exec sp_executesql N'CREATE TABLE [dbo].[Dummy] ( [EmployeeId] INT NOT NULL PRIMARY KEY, [Age] int NOT NULL default ((1)))';");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=False /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(dbo)\";";
            

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

           var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'TableName';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes where name = 'CIDX_ColumnStoreIndexName';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.default_constraints where is_system_named = 'true';");
            Assert.AreEqual(1, count, proc.Messages);

            Assert.Pass(proc.Messages);

        }

        [Test]
        public void IgnoreSchema_Filter_Applies_To_UnnamedCheckConstraint()
        {
            //create a second schema, with tables and a check constraint.  we will attempt to ignore this schema and ensure the unnamed CHECK constraint is not dropped
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");

            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'refers') exec sp_executesql N'DROP table blah.refers';");
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            _gateway.RunQuery("exec sp_executesql N'CREATE table blah.products(product_id int)';");
            _gateway.RunQuery("exec sp_executesql N'CREATE table blah.refers(refer_id int, product_id int, age int CHECK (age > 1), count int CONSTRAINT CK_Count_Positive CHECK (count > 0))';");
            int count = _gateway.GetInt("select count(*) From sys.check_constraints where parent_object_id = OBJECT_ID('blah.refers')");
            Assert.AreEqual(2, count, "Expected constraint to exist before dacpac");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema!(dbo)\";";


            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'products';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'refers';");
            Assert.AreEqual(1, count, proc.Messages);


            count = _gateway.GetInt("select count(*) From sys.check_constraints where parent_object_id = OBJECT_ID('blah.refers')");
            Assert.AreEqual(2, count, proc.Messages);

            //cleanup test tables
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'refers') exec sp_executesql N'DROP table blah.refers';");
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            Assert.Pass(proc.Messages);

        }
        [Test]
        public void IgnoreSchema_Filter_Applies_To_UnnamedDefaultConstraint()
        {
            //create a second schema, with tables and a default constraint.  we will attempt to ignore this schema and ensure the unnamed default constraint is not dropped
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");

            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            _gateway.RunQuery("exec sp_executesql N'CREATE table blah.products(product_id int, age int DEFAULT ((1)), count int CONSTRAINT DF_Count DEFAULT ((0)) )';");

            var count = _gateway.GetInt("select count(*) From sys.default_constraints where parent_object_id = OBJECT_ID('blah.products')");
            Assert.AreEqual(2, count, "Expected constraint to exist before dacpac");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema!(dbo)\";";


            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'products';");
            Assert.AreEqual(1, count, proc.Messages);


            count = _gateway.GetInt("select count(*) From sys.default_constraints where parent_object_id = OBJECT_ID('blah.products')");
            Assert.AreEqual(2, count, proc.Messages);

            //cleanup test tables
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            Assert.Pass(proc.Messages);

        }
        [Test]
        public void IgnoreSchema_Filter_Applies_To_UnnamedPrimaryKey()
        {
            //create a second schema, with tables and a key constraint.  we will attempt to ignore this schema and ensure the unnamed key constraint is not dropped
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");

            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            _gateway.RunQuery("exec sp_executesql N'CREATE table blah.products(product_id int primary key, age int, count int)';");

            var count = _gateway.GetInt("select count(*) From sys.key_constraints where parent_object_id = OBJECT_ID('blah.products')");
            Assert.AreEqual(1, count, "Expected constraint to exist before dacpac");

            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema!(dbo)\";";


            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'products';");
            Assert.AreEqual(1, count, proc.Messages);


            count = _gateway.GetInt("select count(*) From sys.key_constraints where parent_object_id = OBJECT_ID('blah.products')");
            Assert.AreEqual(1, count, proc.Messages);

            //cleanup test tables
            _gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'products') exec sp_executesql N'DROP table blah.products';");
            Assert.Pass(proc.Messages);
        }
    }
}
