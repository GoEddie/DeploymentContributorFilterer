//need tou pdate 2 version numbers

//should blog about what happens when you get a table change and detail what effect this new option will have
//would like to also blog about how to change the default table migration script to a new one that does it in batches

using System.Configuration;
using NUnit.Framework;
using System.IO;


namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class KeepColumnsTests
    {
        private readonly SqlGateway _gateway =
            new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof (string)) as string);

        [Test]
        public void Column_Is_Not_Dropped_When_Column_In_Dacpac_Is_Added()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(max)); ';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'Name' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Columns_Are_Badly_Renamed_()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [Employee________Id] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(max));';");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Columns_Are_ReOrdered()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(max));';");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Existing_Columns_Are_In_The_Incorrect_Order_()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(max));';");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'ohwahweewah') exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [Name] VARCHAR(25) NOT NULL, [ohwahweewah] varchar(max)); ';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Name_Is_To_Keep_And_Constraint_Is_Dropped()
        {
            //exec sp_executesql N'alter table Employees add constraint [cs_abcd] check (Name like ''%%''); '
            _gateway.RunQuery(
                "IF NOT EXISTS (select * from sys.objects where name = 'cs_abcd') exec sp_executesql N'alter table Employees add constraint [cs_abcd] check (Name like ''%%'');';");
            var count = _gateway.GetInt("select COUNT(*) from sys.objects where name = 'cs_abcd';");
            Assert.AreEqual(1, count);

            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("select COUNT(*) from sys.objects where name = 'cs_abcd';");
            Assert.AreEqual(0, count, proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Trigger_Is_Removed_From_Table()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(max));';");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.triggers where name = 'gh';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:localhost /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.triggers where name = 'gh';");
            Assert.AreEqual(0, count);


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
        }
    }
}