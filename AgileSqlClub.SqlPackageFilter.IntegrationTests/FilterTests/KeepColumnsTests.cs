using System;
using System.Configuration;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class KeepColumnsTests
    {
        private readonly SqlGateway _gateway =
            new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof (string)) as string);

        [Test]
        public void Column_Is_Not_Dropped_When_Name_Is_To_Keep()
        {
            _gateway.RunQuery(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'ohwahweewah') exec sp_executesql N'drop table employees; create table Employees([EmployeeId] INT NOT NULL PRIMARY KEY, [Name] VARCHAR(25) NOT NULL, [ohwahweewah] varchar(max)); ';");
            var count = _gateway.GetInt("SELECT COUNT(*) FROM sys.columns where name = 'ohwahweewah';");
            Assert.AreEqual(1, count);
            

            var args =
                "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway(".\\SqlPackage.exe\\SqlPackage.exe", args);
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
                "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor " +
                " /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True " +
                "/p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=KeepTableColumns(Employees)\" /p:AllowIncompatiblePlatform=true";

            var proc = new ProcessGateway(".\\SqlPackage.exe\\SqlPackage.exe", args);
            proc.Run();
            proc.WasDeploySuccess();

            count = _gateway.GetInt("select COUNT(*) from sys.objects where name = 'cs_abcd';");
            Assert.AreEqual(0, count, proc.Messages);

        }
    }

}