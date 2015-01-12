using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AgileSqlClub.SqlPackageFilter.IntegrationTests
{
    [TestFixture]
    public class IgnoreTests
    {
        [Test]
        public void Schema_Is_Not_Dropped_When_Name_Is_Ignored()
        {
            var gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof (string)) as string);
            gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'blah') exec sp_executesql N'CREATE SCHEMA blah';");
            gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bloobla') exec sp_executesql N'CREATE table blah.bloobla(id int)';");
            
            var args =
            "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(blah)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True";
            var p = Process.Start(".\\SqlPackage.exe\\SqlPackage.exe", args);
                p.WaitForExit(Timeout.Infinite);

            var tableCount = gateway.GetInt("SELECT COUNT(*) FROM sys.schemas where name = 'blah';");

            Assert.AreEqual(1, tableCount);

        }

        [Test]
        public void Schema_Is_Not_Created_When_Name_Is_Ignored()
        {
            var gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);
            
            gateway.RunQuery("IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')\r\nBEGIN DROP table dbo.Employees\r\nEND;");

            var args =
            "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreSchema(dbo)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True";
            var p = Process.Start(".\\SqlPackage.exe\\SqlPackage.exe", args);
            p.WaitForExit(Timeout.Infinite);

            var tableCount = gateway.GetInt("SELECT COUNT(*) FROM sys.tables where name = 'Employees';");

            Assert.AreEqual(0, tableCount);
        }

        [Test]
        public void Stored_Procedures_Are_Ignored_When_Objects_Ignored()
        {
            var gateway = new SqlGateway(new AppSettingsReader().GetValue("ConnectionString", typeof(string)) as string);
            gateway.RunQuery("IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'proc_to_ignore')\r\nBEGIN exec sp_executesql N'create procedure proc_to_ignore as select 1;'\r\nEND;");

            var procCount = gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount);

            var args =
            "/Action:Publish /TargetServerName:localhost /SourceFile:DacPac.dacpac /p:AdditionalDeploymentContributors=AgileSqlClub.DeploymentFilterContributor /p:AdditionalDeploymentContributorArguments=\"SqlPackageFilter=IgnoreType(.*Proced.*)\"  /TargetDatabaseName:Filters /p:DropObjectsNotInSource=True";
            var processInfo = new ProcessStartInfo(".\\SqlPackage.exe\\SqlPackage.exe", args);
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            
            var p = Process.Start(processInfo);
            p.WaitForExit(Timeout.Infinite);

            

            procCount = gateway.GetInt("SELECT COUNT(*) FROM sys.procedures where name = 'proc_to_ignore';");

            Assert.AreEqual(1, procCount, p.StandardOutput.ReadToEnd() + " Errors: " + p.StandardError.ReadToEnd());

        }

    }

}
