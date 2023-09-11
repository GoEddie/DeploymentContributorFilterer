//need tou pdate 2 version numbers

//should blog about what happens when you get a table change and detail what effect this new option will have
//would like to also blog about how to change the default table migration script to a new one that does it in batches

using System;
using System.Configuration;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework.Internal;


namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class KeepColumnsTestsWithData
    {
        private static Random employeeId = new Random();
        private readonly SqlGateway _gateway =
            new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof (string)) as string);

        [Test]
        public void Can_1_Publish_reset_to_a_known_state()
        {
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees') exec sp_executesql N'delete from dbo.Employees';");


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")}" +
                $" /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:AllowIncompatiblePlatform=true /p:BlockOnPossibleDataLoss=False ";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();
            Assert.Pass(proc.Messages);

        }
        [Test]
        public void Column_Is_Not_Dropped_When_Column_In_Dacpac_Is_Added()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(24)); ';");
            _gateway.RunQuery(
                "drop index if exists dbo.Employees.NC_qq_employeeId; create nonclustered index NC_qq_employeeId on dbo.Employees(EmployeeId);");
            _gateway.RunQuery($"insert into Employees([EmployeeId]) select {employeeId.Next()}");

            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True /p:BlockOnPossibleDataLoss=False " +
                " /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt( // did we add the column Name?
                    "SELECT COUNT(*) FROM sys.columns where name = 'Name' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);

            var count2 = _gateway.GetInt( // did we drop the column we have above?
                "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count2);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);


            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Columns_Are_Badly_Renamed_()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [Employee________Id] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(24));';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");
            _gateway.RunQuery($"insert into Employees([Employee________Id]) select {employeeId.Next()}");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);
            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Columns_Are_ReOrdered()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(10));';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");
            _gateway.RunQuery($"insert into Employees([EmployeeId]) select {employeeId.Next()}");

            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Existing_Columns_Are_In_The_Incorrect_Order_()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(25));';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery($"insert into Employees([EmployeeId]) select {employeeId.Next()}");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'ohwahweewah') exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [Name] VARCHAR(25) NOT NULL, [ohwahweewah] varchar(25)); ';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId           _gateway.RunQuery($"insert into Employees([EmployeeId],[Name]) select {employeeId.Next()}, 'bob'");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Index_Is_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'ohwahweewah') exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [Name] VARCHAR(25) NOT NULL, [ohwahweewah] varchar(24)); ';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId,Name)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,Name)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery($"insert into Employees([EmployeeId],[Name]) select {employeeId.Next()}, 'bob'");

            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);
            
            Assert.Pass(proc.Messages);
        }


        [Test]
        public void Index_Is_Updated_When_Name_Is_To_Keep()
        {
            // correct table here
            _gateway.RunQuery(
                "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'employees') exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [Name] VARCHAR(25) NOT NULL, [ohwahweewah] varchar(24)); ';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId,Name)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,Name)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery($"insert into Employees([EmployeeId],Name) select {employeeId.Next()},'bob'");

            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId';");
            Assert.AreEqual(1, count, proc.Messages);

            Assert.Pass(proc.Messages);
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
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway( Path.Combine(TestContext.CurrentContext.TestDirectory,   "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("select COUNT(*) from sys.objects where name = 'cs_abcd';");
            Assert.AreEqual(0, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Trigger_Is_Removed_From_Table()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(10));';");
            _gateway.RunQuery($"insert into Employees([EmployeeId]) select {employeeId.Next()}");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId


            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.triggers where name = 'gh';");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
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

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }

        [Test]
        public void Column_Is_Not_Dropped_When_Columns_Named_By_Wildcard()
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(10));';");
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'NC_qq_employee_employeeid') Drop index NC_qq_employee_employeeid on dbo.Employees;
                  exec sp_executesql N'create nonclustered index NC_qq_employee_employeeid on dbo.Employees(EmployeeId)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery(
                @"IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_EmployeeId') Drop index IX_Employees_EmployeeId on dbo.Employees;
                  exec sp_executesql N'create nonclustered index IX_Employees_EmployeeId on dbo.Employees(EmployeeId,ohwahweewah)';"); // wrong index columns here, should just be the EmployeeId
            _gateway.RunQuery($"insert into Employees([EmployeeId],[Name]) select {employeeId.Next()}, 'bob'");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);


            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(.*)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count, proc.Messages);

            count = _gateway.GetInt("SELECT COUNT(*) FROM sys.indexes WHERE name = 'NC_qq_employee_name';");
            Assert.AreEqual(0, count, proc.Messages);

            Assert.Pass(proc.Messages);
        }


        [TestCase("", true)] // null matches all schemas - leading
        [TestCase("dbo", true)] // dbo matches what we keep
        [TestCase("bob", false)] // bob does not.
        public void Column_Is_Not_Dropped_When_Columns_Named_By_Schema(string schema, bool isRetained)
        {
            _gateway.RunQuery(
                " exec sp_executesql N'IF EXISTS(SELECT * FROM SYS.TABLES WHERE NAME = ''Employees'') begin\r\n drop table employees;\r\nend \r\n create table Employees(name varchar(max), [EmployeeId] INT NOT NULL PRIMARY KEY, [ohwahweewah] varchar(24));';");
            _gateway.RunQuery($"insert into Employees([EmployeeId],[Name]) select {employeeId.Next()}, 'bob'");
            _gateway.RunQuery(
                " exec sp_executesql N'create trigger gh on Employees AFTER INSERT AS select 100';");
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            _gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bloobla') exec sp_executesql N'CREATE table blah.bloobla(id int)';");


            var count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(1, count);

            // could add ;SqlPackageLogging=Info after SqlPackageFilter to aid diagnosis
            var args =
                $"/Action:Publish /TargetServerName:(localdb)\\Filter /SourceFile:{Path.Combine(TestContext.CurrentContext.TestDirectory, "Dacpac.Dacpac")} /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True  /p:BlockOnPossibleDataLoss=False " +
                $"/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns({schema},Employees)\" /p:AllowIncompatiblePlatform=true /p:GenerateSmartDefaults=true";

            var proc = new ProcessGateway(Path.Combine(TestContext.CurrentContext.TestDirectory, "SqlPackage.exe\\SqlPackage.exe"), args);
            proc.Run();
            proc.WasDeploySuccess();


            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah' and object_id = object_id('Employees');");
            Assert.AreEqual(isRetained?1:0, count, proc.Messages);
            count =
                _gateway.GetInt(
                    "SELECT COUNT(*) FROM sys.columns where name = 'id' and object_id = object_id('bloobla');");
            Assert.AreEqual(0, count, proc.Messages);
            Assert.Pass(proc.Messages);
        }

    }
}